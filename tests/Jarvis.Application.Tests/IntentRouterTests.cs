using Jarvis.Application.Services;

namespace Jarvis.Application.Tests;

public sealed class IntentRouterTests
{
    private readonly IntentRouter _router = new(new AllowlistCommandCatalog());

    [Fact]
    public void Classify_ShouldReturnChat_WhenInputIsRegularChat()
    {
        var result = _router.Classify("Explique SOLID em uma frase.");

        Assert.Equal(Jarvis.Application.Models.InputIntent.Chat, result.Intent);
        Assert.Null(result.NormalizedCommand);
    }

    [Fact]
    public void Classify_ShouldReturnCommand_WhenInputIsAllowedCommand()
    {
        var result = _router.Classify("/cmd abrir_notepad");

        Assert.Equal(Jarvis.Application.Models.InputIntent.Command, result.Intent);
        Assert.Equal("abrir_notepad", result.NormalizedCommand);
    }

    [Fact]
    public void Classify_ShouldReturnUnknownCommand_WhenInputCommandIsNotAllowed()
    {
        var result = _router.Classify("/cmd desligar_pc");

        Assert.Equal(Jarvis.Application.Models.InputIntent.UnknownCommand, result.Intent);
        Assert.Equal("desligar_pc", result.NormalizedCommand);
    }

    [Fact]
    public void Classify_ShouldNormalizeCasing_WhenCommandUsesUppercase()
    {
        var result = _router.Classify("   /CMD ABRIR_CALCULADORA   ");

        Assert.Equal(Jarvis.Application.Models.InputIntent.Command, result.Intent);
        Assert.Equal("abrir_calculadora", result.NormalizedCommand);
    }

    [Fact]
    public void Classify_ShouldNormalizeSpacing_WhenInputHasExtraSpaces()
    {
        var result = _router.Classify("   /cmd    mostrar_data_hora   ");

        Assert.Equal(Jarvis.Application.Models.InputIntent.Command, result.Intent);
        Assert.Equal("mostrar_data_hora", result.NormalizedCommand);
    }
}
