# SCUM Quest Editor

Tool for creating custom quests in the video game SCUM.

A lot of the safeties have been turned off and limits disabled.  
This means it's possible to create quests that don't conform to the standards set out in the documentation.  
It's up to the end user to know these limits.  

The documentation in question:
https://docs.google.com/document/d/1B1qooypdebE2xvJ33cb-BIH5MEEsvi9w4v-vrgcYO1k

This tool is provided as-is with no warranty.

This is an unofficial, community made tool not associated with gamepires or the scum development team.  
It is free for non-commercial use.

You can customize the default file saving format  
Available placeholders:
* {tier} - Quest tier (1, 2, 3, 4)
* {trader} - Trader code (AR, BK, BA, BT, DC, GG, HM, HT, MC)
* {title} - Quest title with spaces replaced by underscores

Auto builds at midnight UTC, check the [releases](https://github.com/ntoff/SCUMQuestEditor/releases) section for updates before posting issues.

Includes "open with" support and support for opening quest files by dragging them onto the main window.  
It attempts to do some basic validation by making sure certain elements aren't missing or empty but doesn't perform full json validation  
