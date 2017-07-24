# XamUBot

## Get the Codez
* clone the repo 
* Build the solution and launch it... this should fire up in your browser and the bot should be running with an endpoint tha looks something like `localhost:3979`

# Testing Locally

## Get the Bot Emulator to test locally
* Navigate to https://github.com/Microsoft/BotFramework-Emulator/releases/latest
* Download the setup... it'll have a name like `botframework-emulator-Setup-3.5.29.exe`
* Install that puppy.

## Connect locally 
* Run up the Bot Emulator
* Type in your endpoint URL, remembering to include the `/api/messages` at the end. So it should look like: `http://localhost:3979/api/messages`
* You will be prompted for the `Micrsoft App ID` and the `Microsoft App Password`.  You can find these values in the `Web.config` of the `XamUBot` project.
* Throw those values in and you should be good to go.

:)
