# AudioSelector

This Windows only program runs in the traybar and allows the user to quickly switch between setups/combinations of different audio devices.

Use the config.xml to list multiple Setups/Options and define the input and output devices to select for the option in them. The device name has to correspond to the name in 'Control-Panel -> Sound'.

Uses nircmd to do the actual switching so it basically is a traybar wrapper around nircmd.
https://www.nirsoft.net/utils/nircmd.html
