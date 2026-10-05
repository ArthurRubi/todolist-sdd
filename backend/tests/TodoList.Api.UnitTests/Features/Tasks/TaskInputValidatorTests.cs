using TodoList.Api.Features.Tasks;
using TodoList.Api.Features.Tasks.Create;

namespace TodoList.Api.UnitTests.Features.Tasks;

public sealed class TaskInputValidatorTests
{
    [Fact]
    public void Minimal_input_is_normalized_with_defaults()
    {
        var result = TaskInputValidator.Validate(new CreateTaskRequest("  Preparar reunião  "));

        Assert.True(result.IsValid);
        Assert.NotNull(result.Value);
        Assert.Equal("Preparar reunião", result.Value.Title);
        Assert.Null(result.Value.Description);
        Assert.Equal(TaskPriority.None, result.Value.Priority);
        Assert.Null(result.Value.DueDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_or_whitespace_title_is_rejected(string title)
    {
        var result = TaskInputValidator.Validate(new CreateTaskRequest(title));

        Assert.False(result.IsValid);
        Assert.Contains("title", result.Errors);
    }

    [Fact]
    public void Internal_spaces_and_multiline_description_are_preserved()
    {
        var result = TaskInputValidator.Validate(new CreateTaskRequest(
            "  Revisar   roteiro  ",
            "Primeira linha\n\n  Segunda linha",
            "high",
            new DateOnly(2025, 1, 2)));

        Assert.True(result.IsValid);
        Assert.Equal("Revisar   roteiro", result.Value!.Title);
        Assert.Equal("Primeira linha\n\n  Segunda linha", result.Value.Description);
        Assert.Equal(TaskPriority.High, result.Value.Priority);
        Assert.Equal(new DateOnly(2025, 1, 2), result.Value.DueDate);
    }

    [Fact]
    public void Boundary_lengths_are_accepted()
    {
        var result = TaskInputValidator.Validate(new CreateTaskRequest(
            new string('T', 200),
            new string('D', 10_000),
            "urgent"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Values_above_limits_are_rejected_per_field()
    {
        var result = TaskInputValidator.Validate(new CreateTaskRequest(
            new string('T', 201),
            new string('D', 10_001),
            "unexpected"));

        Assert.False(result.IsValid);
        Assert.Equal(["O título deve ter no máximo 200 caracteres."], result.Errors["title"]);
        Assert.Equal(["A descrição deve ter no máximo 10.000 caracteres."], result.Errors["description"]);
        Assert.Equal(["A prioridade informada é inválida."], result.Errors["priority"]);
    }

    [Fact]
    public void A_past_due_date_is_valid()
    {
        var result = TaskInputValidator.Validate(new CreateTaskRequest(
            "Entregar documento",
            DueDate: new DateOnly(2000, 1, 1)));

        Assert.True(result.IsValid);
        Assert.Equal(new DateOnly(2000, 1, 1), result.Value!.DueDate);
    }
}
