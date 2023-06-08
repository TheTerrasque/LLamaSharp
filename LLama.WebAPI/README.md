# WebApi using Llama

This is a simple webapi implementing a very basic openai compatible api, and uses llama.cpp via LlamaSharp as LLM.

Look at http://localhost:62211/swagger/index.html for API information

## Settings

Local settings can be configured via `appsettings.Development.json`.

### Logging

Basic Serilog logging is implemented. To expand on it use the base object `Serilog` in `appsettings.Development.json`

If you want to say.. Use [Seq](https://datalust.co/seq) as logging backend and enable debug messages in Seq, but still have basic logging in console, you can set it up like this:

    {
        "Serilog": {
        "MinimumLevel": "Debug",
            "WriteTo": [
                { "Name": "Console", "Args": { 
                    "restrictedToMinimumLevel": "Information" } 
                },
                { "Name": "Seq", 
                    "Args": { "serverUrl": "http://localhost:5341" }
                }
            ]
        },
    }

For more details, refer to [Serilog](https://github.com/serilog/serilog/wiki/Configuration-Basics) and [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) documentation.

### Model settings

For ChatService model settings, create a section called `ChatService` in `appsettings.Development.json`.

For example, to set model path you can do 

    {
        "ChatService": {
            "Model": "C:\\Path\\To\\Model.ggml"
        }
    }

For overview of all settings and their default value, look at `BaseChatServiceOptions` in `Services\BaseChatService.cs`