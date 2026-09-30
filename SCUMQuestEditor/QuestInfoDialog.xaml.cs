using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SCUMQuestEditor
{
    public partial class QuestInfoDialog : Window
    {
        private readonly Dictionary<string, TabItem> _tabMap = new();

        private static readonly Regex s_orderedListRegex = new(@"^\s*\d+\.\s", RegexOptions.Compiled);
        private static readonly Regex s_matchOrderedList = new(@"^\s*(\d+)\.\s*", RegexOptions.Compiled);
        private static readonly Regex s_replaceOrderedList = new(@"^\s*\d+\.\s*", RegexOptions.Compiled);
        private static readonly Regex s_bulletListRegex = new(@"^\s*-\s", RegexOptions.Compiled);
        private static readonly Regex s_replaceBulletList = new(@"^\s*-\s*", RegexOptions.Compiled);

        private static readonly (string title, string fileName)[] s_tabDefinitions = new[]
        {
            ("General Info", "GeneralInfo.md"),
            ("Quest Basics", "QuestBasics.md"),
            ("Rewards", "Rewards.md"),
            ("Conditions", "Conditions.md"),
            ("Location Info", "LocationInfo.md"),
            ("Important Limits", "ImportantLimits.md")
        };

        private static readonly Lazy<Dictionary<string, string>> s_markdownCache = new(() =>
        {
            string legendPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_data", "legend");
            var cache = new Dictionary<string, string>();

            foreach (var (title, fileName) in s_tabDefinitions)
            {
                string mdPath = Path.Combine(legendPath, fileName);
                if (File.Exists(mdPath))
                {
                    cache[title] = File.ReadAllText(mdPath);
                }
            }

            return cache;
        });

        public QuestInfoDialog()
        {
            InitializeComponent();
            this.Loaded += QuestInfoDialog_Loaded;
        }

        private void QuestInfoDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= QuestInfoDialog_Loaded;
            LoadLegendTabs();
        }

        private void LoadLegendTabs()
        {
            var contentCache = s_markdownCache.Value;

            foreach (var (title, fileName) in s_tabDefinitions)
            {
                if (!contentCache.TryGetValue(title, out string? content))
                    continue;

                var tab = new TabItem
                {
                    Header = title,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                };

                var flowDocument = new FlowDocument();
                flowDocument.FontFamily = new FontFamily("Segoe UI, Arial, Verdana, Helvetica, sans-serif");
                flowDocument.FontSize = 12;
                flowDocument.ColumnWidth = double.MaxValue;
                flowDocument.TextAlignment = TextAlignment.Left;

                RenderMarkdown(flowDocument, content);

                var scrollViewer = new ScrollViewer
                {
                    Content = flowDocument,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                };

                tab.Content = scrollViewer;
                InfoTabs.Items.Add(tab);
                _tabMap[title] = tab;
            }

            if (InfoTabs.Items.Count > 0)
            {
                InfoTabs.SelectedIndex = 0;
            }
        }

        private void RenderMarkdown(FlowDocument doc, string content)
        {
            string[] lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            bool inCodeBlock = false;
            var codeLines = new List<string>();
            var listItems = new List<(int indent, bool isOrdered, string? number, string text)>();
            int currentListBaseIndent = 0;

            int GetIndentLevel(string line)
            {
                int count = 0;
                foreach (char c in line)
                {
                    if (c == ' ') count++;
                    else break;
                }
                return count / 2;
            }

            void FlushList()
            {
                if (listItems.Count == 0) return;

                Paragraph para = new Paragraph();
                para.FontSize = 12;
                para.Margin = new Thickness(20, 2, 0, 4);
                para.Foreground = GetTextBrush();

                for (int i = 0; i < listItems.Count; i++)
                {
                    var (indent, isOrdered, number, text) = listItems[i];
                    int depth = Math.Max(0, indent - currentListBaseIndent);

                    for (int d = 0; d < depth; d++)
                    {
                        Run indentRun = new Run("\u2003 ");
                        indentRun.FontSize = 12;
                        para.Inlines.Add(indentRun);
                    }

                    if (isOrdered)
                    {
                        Run prefix = new Run($"{number ?? "1"}. ")
                        {
                            FontWeight = FontWeights.Bold,
                            Foreground = GetListPrefixBrush()
                        };
                        para.Inlines.Add(prefix);
                    }
                    else
                    {
                        Run bullet = new Run("\u2022 ")
                        {
                            FontWeight = FontWeights.Bold,
                            Foreground = GetListPrefixBrush()
                        };
                        para.Inlines.Add(bullet);
                    }

                    foreach (Inline inline in ParseInline(text, isListItem: true))
                        para.Inlines.Add(inline);

                    para.Inlines.Add(new LineBreak());
                }

                doc.Blocks.Add(para);
                listItems.Clear();
            }

            void FlushCodeBlock()
            {
                if (codeLines.Count == 0) return;

                TextBox codeBox = new TextBox
                {
                    Text = string.Join("\n", codeLines),
                    IsReadOnly = true,
                    Background = GetCodeBackgroundBrush(),
                    Foreground = GetCodeForegroundBrush(),
                    FontFamily = new FontFamily("Consolas"),
                    FontSize = 12,
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 8, 0, 8),
                    TextWrapping = TextWrapping.Wrap,
                    BorderThickness = new Thickness(0),
                    AcceptsReturn = false
                };

                doc.Blocks.Add(new BlockUIContainer(codeBox));
                codeLines.Clear();
            }

            void FlushPending()
            {
                if (listItems.Count > 0) FlushList();
            }

            foreach (var rawLine in lines)
            {
                string line = rawLine.TrimEnd();

                if (line.StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        FlushCodeBlock();
                        inCodeBlock = false;
                    }
                    else
                    {
                        FlushPending();
                        inCodeBlock = true;
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    codeLines.Add(line);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    FlushPending();
                    continue;
                }

                if (line.StartsWith("###### "))
                {
                    FlushPending();
                    doc.Blocks.Add(CreateHeading(line.Substring(7), 5));
                }
                else if (line.StartsWith("##### "))
                {
                    FlushPending();
                    doc.Blocks.Add(CreateHeading(line.Substring(6), 4));
                }
                else if (line.StartsWith("#### "))
                {
                    FlushPending();
                    doc.Blocks.Add(CreateHeading(line.Substring(5), 3));
                }
                else if (line.StartsWith("### "))
                {
                    FlushPending();
                    doc.Blocks.Add(CreateHeading(line.Substring(4), 2));
                }
                else if (line.StartsWith("## "))
                {
                    FlushPending();
                    doc.Blocks.Add(CreateHeading(line.Substring(3), 1));
                }
                else if (line.StartsWith("# "))
                {
                    FlushPending();
                    doc.Blocks.Add(CreateHeading(line.Substring(2), 0));
                }
                else if (s_orderedListRegex.IsMatch(line))
                {
                    int indent = GetIndentLevel(line);
                    var match = s_matchOrderedList.Match(line);
                    string numStr = match.Success ? match.Groups[1].Value : string.Empty;
                    string text = s_replaceOrderedList.Replace(line, "").TrimStart(' ', '\t');
                    if (listItems.Count == 0) currentListBaseIndent = indent;
                    listItems.Add((indent, true, numStr, text));
                }
                else if (s_bulletListRegex.IsMatch(line))
                {
                    int indent = GetIndentLevel(line);
                    string text = s_replaceBulletList.Replace(line, "").TrimStart(' ', '\t');
                    if (listItems.Count == 0) currentListBaseIndent = indent;
                    listItems.Add((indent, false, null, text));
                }
                else
                {
                    FlushPending();
                    var para = new Paragraph { FontSize = 12 };
                    foreach (Inline inline in ParseInline(line))
                        para.Inlines.Add(inline);
                    para.Margin = new Thickness(0, 2, 0, 4);
                    para.Foreground = GetTextBrush();
                    doc.Blocks.Add(para);
                }
            }

            FlushCodeBlock();
            FlushList();
        }

        private Block CreateHeading(string text, int level)
        {
            Brush color = GetHeadingBrush();
            Paragraph par = new Paragraph();
            par.Margin = new Thickness(0, level == 0 ? 16 : 12, 0, 4);
            par.FontWeight = FontWeights.Bold;

            switch (level)
            {
                case 0: par.FontSize = 22; break;
                case 1: par.FontSize = 18; break;
                case 2: par.FontSize = 16; break;
                case 3: par.FontSize = 14; break;
                case 4: par.FontSize = 13; break;
                default: par.FontSize = 12; break;
            }

            foreach (Inline inline in ParseInline(text))
            {
                if (inline is Bold b)
                {
                    foreach (Inline child in b.Inlines)
                    {
                        if (child is Span s)
                        {
                            s.Foreground = color;
                            par.Inlines.Add(s);
                        }
                        else if (child is Run r)
                        {
                            par.Inlines.Add(new Run(r.Text) { Foreground = color });
                        }
                    }
                }
                else if (inline is Span span)
                {
                    span.Foreground = color;
                    par.Inlines.Add(span);
                }
                else if (inline is Run r2)
                {
                    par.Inlines.Add(new Run(r2.Text) { Foreground = color });
                }
            }

            return par;
        }

        private string GetInlineText(Inline inline)
        {
            return inline switch
            {
                Bold b => string.Concat(b.Inlines.Select(GetInlineText)),
                Italic it => string.Concat(it.Inlines.Select(GetInlineText)),
                Run r => r.Text,
                Span s => string.Concat(s.Inlines.Select(GetInlineText)),
                _ => ""
            };
        }

        private IEnumerable<Inline> ParseInline(string text, bool isListItem = false)
        {
            int i = 0;
            var plainChars = new System.Text.StringBuilder();
            var result = new List<Inline>();

            void FlushPlain()
            {
                if (plainChars.Length > 0)
                {
                    result.Add(new Run(plainChars.ToString())
                    {
                        Foreground = isListItem ? GetTextBrush() : GetTextBrush()
                    });
                    plainChars.Clear();
                }
            }

            while (i < text.Length)
            {
                if (text[i] == '`' && i + 1 < text.Length)
                {
                    FlushPlain();
                    int end = text.IndexOf('`', i + 1);
                    if (end > i)
                    {
                        result.Add(new Run(text.Substring(i + 1, end - i - 1))
                        {
                            FontFamily = new FontFamily("Consolas"),
                            FontWeight = FontWeights.SemiBold,
                            Foreground = GetHeadingBrush()
                        });
                        i = end + 1;
                        continue;
                    }
                }

                if (i + 1 < text.Length && text[i] == '*' && text[i + 1] == '*')
                {
                    FlushPlain();
                    int end = text.IndexOf("**", i + 2);
                    if (end > i + 1)
                    {
                        Bold bold = new Bold();
                        string inner = text.Substring(i + 2, end - i - 2);
                        foreach (Inline inline in ParseInline(inner, isListItem))
                        {
                            if (inline is Run run)
                            {
                                var r = new Run(run.Text)
                            {
                                FontWeight = FontWeights.Bold,
                                Foreground = isListItem
                                    ? GetTextBrush()
                                    : GetHeadingBrush()
                            };
                                if (run.FontFamily != null) r.FontFamily = run.FontFamily;
                                if (run.Foreground != null && isListItem) r.Foreground = GetTextBrush();
                                bold.Inlines.Add(r);
                            }
                        }
                        result.Add(bold);
                        i = end + 2;
                        continue;
                    }
                }

                if (text[i] == '*' && i + 1 < text.Length)
                {
                    FlushPlain();
                    int end = text.IndexOf('*', i + 1);
                    if (end > i + 1)
                    {
                        Italic italic = new Italic();
                        string inner = text.Substring(i + 1, end - i - 1);
                        foreach (Inline inline in ParseInline(inner, isListItem))
                        {
                            if (inline is Run run)
                            {
                                var r = new Run(run.Text)
                            {
                                FontStyle = FontStyles.Italic,
                                Foreground = isListItem ? GetTextBrush() : GetItalicBrush()
                            };
                                if (run.FontFamily != null) r.FontFamily = run.FontFamily;
                                italic.Inlines.Add(r);
                            }
                        }
                        result.Add(italic);
                        i = end + 1;
                        continue;
                    }
                }

                if (text[i] == '[' && i + 1 < text.Length)
                {
                    FlushPlain();
                    int bracketEnd = text.IndexOf(']', i);
                    if (bracketEnd > i && bracketEnd + 1 < text.Length && text[bracketEnd + 1] == '(')
                    {
                        int parenEnd = text.IndexOf(')', bracketEnd + 2);
                        if (parenEnd > bracketEnd + 1)
                        {
                            string linkText = text.Substring(i + 1, bracketEnd - i - 1);
                            string url = text.Substring(bracketEnd + 2, parenEnd - bracketEnd - 2);

                            Hyperlink hyperlink = new Hyperlink(new Run(linkText))
                            {
                                NavigateUri = new Uri(url),
                                Foreground = GetLinkBrush(),
                                Cursor = System.Windows.Input.Cursors.Hand
                            };
                            hyperlink.Click += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                            result.Add(hyperlink);
                            i = parenEnd + 1;
                            continue;
                        }
                    }
                }

                plainChars.Append(text[i]);
                i++;
            }

            FlushPlain();
            return result;
        }

        private void InfoTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private bool IsLightMode()
        {
            try
            {
                var bundled = Application.Current.Resources.MergedDictionaries
                    .OfType<MaterialDesignThemes.Wpf.BundledTheme>()
                    .FirstOrDefault();
                return bundled?.BaseTheme == MaterialDesignThemes.Wpf.BaseTheme.Light;
            }
            catch
            {
                return false;
            }
        }

        private SolidColorBrush GetTextBrush()
        {
            try
            {
                var bodyBrush = Application.Current.FindResource("MaterialDesignBody") as SolidColorBrush;
                if (bodyBrush != null) return bodyBrush;
            }
            catch { }
            return new SolidColorBrush(Colors.Black);
        }

        private SolidColorBrush GetHeadingBrush()
        {
            try
            {
                var primaryBrush = Application.Current.FindResource("PrimaryHueMidBrush") as SolidColorBrush;
                if (primaryBrush != null) return primaryBrush;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetHeadingBrush error: {ex.Message}");
            }

            try
            {
                var paletteHelper = new MaterialDesignThemes.Wpf.PaletteHelper();
                var theme = paletteHelper.GetTheme();
                var color = theme.PrimaryMid.Color;
                return new SolidColorBrush(color);
            }
            catch { }

            return new SolidColorBrush(Color.FromArgb(255, 180, 180, 255));
        }

        private SolidColorBrush GetListPrefixBrush()
        {
            try
            {
                var bodyLightBrush = Application.Current.FindResource("MaterialDesignBodyLight") as SolidColorBrush;
                if (bodyLightBrush != null) return bodyLightBrush;
            }
            catch { }
            return new SolidColorBrush(Color.FromArgb(255, 170, 170, 170));
        }

        private SolidColorBrush GetCodeBackgroundBrush()
        {
            try
            {
                var paperBrush = Application.Current.FindResource("MaterialDesignPaper") as SolidColorBrush;
                if (paperBrush != null) return paperBrush;
            }
            catch { }
            return new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
        }

        private SolidColorBrush GetCodeForegroundBrush()
        {
            try
            {
                var bodyBrush = Application.Current.FindResource("MaterialDesignBody") as SolidColorBrush;
                if (bodyBrush != null) return bodyBrush;
            }
            catch { }
            return new SolidColorBrush(Color.FromArgb(255, 166, 226, 47));
        }

        private SolidColorBrush GetItalicBrush()
        {
            try
            {
                var bodyLightBrush = Application.Current.FindResource("MaterialDesignBodyLight") as SolidColorBrush;
                if (bodyLightBrush != null) return bodyLightBrush;
            }
            catch { }
            return new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        }

        private SolidColorBrush GetLinkBrush()
        {
            try
            {
                var secondaryBrush = Application.Current.FindResource("SecondaryHueMidBrush") as SolidColorBrush;
                if (secondaryBrush != null) return secondaryBrush;
            }
            catch { }
            return new SolidColorBrush(Color.FromArgb(255, 130, 200, 255));
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
