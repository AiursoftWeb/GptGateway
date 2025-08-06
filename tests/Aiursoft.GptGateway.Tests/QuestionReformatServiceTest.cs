﻿using Aiursoft.CSTools.Tools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Services;
using Microsoft.Extensions.DependencyInjection;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.GptGateway.Tests;

[TestClass]
public class QuestionReformatServiceTest
{
    private readonly int _port = Network.GetAvailablePort();
    private QuestionReformatService? _service;

    [TestInitialize]
    public async Task CreateServer()
    {
        var server = await AppAsync<TestStartup>([], port: _port);
        _service = server.Services.GetRequiredService<QuestionReformatService>();
    }

    [TestMethod]
    public void TestTrue()
    {
        var mockResponse = new CompletionData
        {
            Choices =
            [
                new()
                {
                    Message = new MessageData
                    {
                        Content = "true"
                    }
                }
            ]
        };
        var score = _service!.ConvertResponseToScore(mockResponse);
        Assert.AreEqual(70, score);
    }

    [TestMethod]
    public void TestFalse()
    {
        var mockResponse = new CompletionData
        {
            Choices =
            [
                new()
                {
                    Message = new MessageData
                    {
                        Content = "false"
                    }
                }
            ]
        };
        var score = _service!.ConvertResponseToScore(mockResponse);
        Assert.AreEqual(0, score);
    }

    [TestMethod]
    public void TestTrueAndFalse()
    {
        var mockResponse = new CompletionData
        {
            Choices =
            [
                new()
                {
                    Message = new MessageData
                    {
                        Content = "true false"
                    }
                }
            ]
        };
        var score = _service!.ConvertResponseToScore(mockResponse);
        Assert.AreEqual(40, score);
    }

    [TestMethod]
    public void TestNoTrueAndFalse()
    {
        var mockResponse = new CompletionData
        {
            Choices =
            [
                new()
                {
                    Message = new MessageData
                    {
                        Content = "nothing"
                    }
                }
            ]
        };
        var score = _service!.ConvertResponseToScore(mockResponse);
        Assert.AreEqual(10, score);
    }
}
