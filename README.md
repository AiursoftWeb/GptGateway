# GptGateway

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitlab.aiursoft.cn/aiursoft/GptGateway/-/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.cn/aiursoft/GptGateway/badges/master/pipeline.svg)](https://gitlab.aiursoft.cn/aiursoft/GptGateway/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.cn/aiursoft/GptGateway/badges/master/coverage.svg)](https://gitlab.aiursoft.cn/aiursoft/GptGateway/-/pipelines)
[![ManHours](https://manhours.aiursoft.cn/r/gitlab.aiursoft.cn/aiursoft/GptGateway.svg)](https://gitlab.aiursoft.cn/aiursoft/GptGateway/-/commits/master?ref_type=heads)
[![Website](https://img.shields.io/website?url=https%3A%2F%2Fquickchat.aiursoft.cn%2F)](https://quickchat.aiursoft.cn)
[![Docker](https://img.shields.io/docker/pulls/aiursoft/gptgateway.svg)](https://hub.docker.com/r/aiursoft/gptgateway)

GptGateway is a OpenAI ChatGPT API gateway. Allowing GPT to call tools like search engines.

## Try

Try a running GptGateway [here](https://quickchat.aiursoft.cn).

## Run in Ubuntu

The following script will install\update this app on your Ubuntu server. Supports Ubuntu 22.04.

Before starting, it's suggested to install ollama first for local testing.

```bash
curl -fsSL https://ollama.com/install.sh | sh
```

This project uses `deepseek-r1:70b` as the default model. Pull it first.

```bash
ollama pull deepseek-r1:70b
```

On your Ubuntu server, run the following command:

```bash
curl -sL https://gitlab.aiursoft.cn/aiursoft/GptGateway/-/raw/master/install.sh | sudo bash
```

Of course, it is suggested that append a custom port number to the command:

```bash
curl -sL https://gitlab.aiursoft.cn/aiursoft/GptGateway/-/raw/master/install.sh | sudo bash -s 8080
```

It will install the app as a systemd service, and start it automatically. Binary files will be located at `/opt/apps`. Service files will be located at `/etc/systemd/system`.

## Run manually

Requirements about how to run

1. Install [.NET 9 SDK](http://dot.net/) and [Node.js](https://nodejs.org/).
2. Execute `npm install` at `wwwroot` folder to install the dependencies.
3. Execute `dotnet run` to run the app.
4. Use your browser to view [http://localhost:5000](http://localhost:5000).

## Run in Microsoft Visual Studio

1. Open the `.sln` file in the project path.
2. Press `F5` to run the app.

## Run in Docker

First, install Docker [here](https://docs.docker.com/get-docker/).

Then run the following commands in a Linux shell:

```bash
image=aiursoft/gptgateway
appName=gptgateway
sudo docker pull $image
sudo docker run -d --name $appName --restart unless-stopped -p 5000:5000 -v /var/www/$appName:/data $image
```

That will start a web server at `http://localhost:5000` and you can test the app.

The docker image has the following context:

| Properties  | Value                               |
|-------------|-------------------------------------|
| Image       | aiursoft/gptgateway |
| Ports       | 5000                                |
| Binary path | /app                                |
| Data path   | /data                               |
| Config path | /data/appsettings.json              |

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you with push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your workflow cruft out of sight.

We're also interested in your feedback on the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.
