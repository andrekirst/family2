using FamilyHub.Api.Common.Infrastructure.Behaviors;
using FamilyHub.Api.Common.Infrastructure.Validation;
using FluentAssertions;
using FluentValidation;
using Mediator;

namespace FamilyHub.Auth.Tests.Common.Infrastructure.Behaviors;

public class ValidationBehaviorTests
{
    private record TestCommand(string Value) : FamilyHub.Common.Application.ICommand<string>;

    private static ValueTask<string> SuccessHandler(TestCommand _, CancellationToken __) =>
        new("handler-executed");

    #region Test Validators

    private sealed class InputValidator : AbstractValidator<TestCommand>, IInputValidator<TestCommand>
    {
        public InputValidator(bool shouldFail = false)
        {
            if (shouldFail)
            {
                RuleFor(x => x.Value)
                    .Must(_ => false)
                    .WithErrorCode("INPUT_INVALID")
                    .WithMessage("Input validation failed");
            }
        }
    }

    private sealed class AuthValidator : AbstractValidator<TestCommand>, IAuthValidator<TestCommand>
    {
        public AuthValidator(bool shouldFail = false)
        {
            if (shouldFail)
            {
                RuleFor(x => x.Value)
                    .Must(_ => false)
                    .WithErrorCode("AUTH_DENIED")
                    .WithMessage("Authorization failed");
            }
        }
    }

    private sealed class BusinessValidator : AbstractValidator<TestCommand>, IBusinessValidator<TestCommand>
    {
        public BusinessValidator(bool shouldFail = false)
        {
            if (shouldFail)
            {
                RuleFor(x => x.Value)
                    .Must(_ => false)
                    .WithErrorCode("ENTITY_NOT_FOUND")
                    .WithMessage("Entity not found");
            }
        }
    }

    private sealed class UndecoratedValidator : AbstractValidator<TestCommand>
    {
        public UndecoratedValidator(bool shouldFail = false)
        {
            if (shouldFail)
            {
                RuleFor(x => x.Value)
                    .Must(_ => false)
                    .WithErrorCode("UNDECORATED_ERROR")
                    .WithMessage("Undecorated validation failed");
            }
        }
    }

    private sealed class TrackingAuthValidator : AbstractValidator<TestCommand>, IAuthValidator<TestCommand>
    {
        public bool WasCalled { get; private set; }

        public TrackingAuthValidator()
        {
            RuleFor(x => x.Value)
                .Must(_ =>
                {
                    WasCalled = true;
                    return true;
                });
        }
    }

    private sealed class TrackingBusinessValidator : AbstractValidator<TestCommand>, IBusinessValidator<TestCommand>
    {
        public bool WasCalled { get; private set; }

        public TrackingBusinessValidator()
        {
            RuleFor(x => x.Value)
                .Must(_ =>
                {
                    WasCalled = true;
                    return true;
                });
        }
    }

    #endregion

    [Fact]
    public async Task Should_execute_handler_when_no_validators()
    {
        var behavior = new ValidationBehavior<TestCommand, string>([]);
        var command = new TestCommand("test");

        var result = await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_execute_handler_when_all_validators_pass()
    {
        var validators = new IValidator<TestCommand>[]
        {
            new InputValidator(shouldFail: false),
            new AuthValidator(shouldFail: false),
            new BusinessValidator(shouldFail: false),
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var result = await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        result.Should().Be("handler-executed");
    }

    [Fact]
    public async Task Should_throw_when_input_validator_fails()
    {
        var validators = new IValidator<TestCommand>[]
        {
            new InputValidator(shouldFail: true),
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(e => e.ErrorCode == "INPUT_INVALID");
    }

    [Fact]
    public async Task Should_short_circuit_when_input_fails_and_not_run_auth_or_business()
    {
        var authValidator = new TrackingAuthValidator();
        var businessValidator = new TrackingBusinessValidator();
        var validators = new IValidator<TestCommand>[]
        {
            new InputValidator(shouldFail: true),
            authValidator,
            businessValidator,
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        authValidator.WasCalled.Should().BeFalse("auth validators should not run when input fails");
        businessValidator.WasCalled.Should().BeFalse("business validators should not run when input fails");
    }

    [Fact]
    public async Task Should_short_circuit_when_auth_fails_and_not_run_business()
    {
        var businessValidator = new TrackingBusinessValidator();
        var validators = new IValidator<TestCommand>[]
        {
            new InputValidator(shouldFail: false),
            new AuthValidator(shouldFail: true),
            businessValidator,
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        businessValidator.WasCalled.Should().BeFalse("business validators should not run when auth fails");
    }

    [Fact]
    public async Task Should_stamp_input_category_on_failures()
    {
        var validators = new IValidator<TestCommand>[]
        {
            new InputValidator(shouldFail: true),
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().AllSatisfy(e =>
            e.CustomState.Should().Be(ValidatorCategory.Input));
    }

    [Fact]
    public async Task Should_stamp_auth_category_on_failures()
    {
        var validators = new IValidator<TestCommand>[]
        {
            new AuthValidator(shouldFail: true),
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().AllSatisfy(e =>
            e.CustomState.Should().Be(ValidatorCategory.Auth));
    }

    [Fact]
    public async Task Should_stamp_business_category_on_failures()
    {
        var validators = new IValidator<TestCommand>[]
        {
            new BusinessValidator(shouldFail: true),
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().AllSatisfy(e =>
            e.CustomState.Should().Be(ValidatorCategory.Business));
    }

    [Fact]
    public async Task Should_run_undecorated_validators_last_with_input_category()
    {
        var validators = new IValidator<TestCommand>[]
        {
            new UndecoratedValidator(shouldFail: true),
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(e => e.ErrorCode == "UNDECORATED_ERROR");
        exception.Which.Errors.Should().AllSatisfy(e =>
            e.CustomState.Should().Be(ValidatorCategory.Input));
    }

    [Fact]
    public async Task Should_run_groups_in_correct_order_regardless_of_registration()
    {
        var authValidator = new TrackingAuthValidator();
        var businessValidator = new TrackingBusinessValidator();
        var validators = new IValidator<TestCommand>[]
        {
            businessValidator,  // registered out of order
            authValidator,       // registered out of order
            new InputValidator(shouldFail: false),
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var result = await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        result.Should().Be("handler-executed");
        authValidator.WasCalled.Should().BeTrue();
        businessValidator.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Should_throw_business_failure_when_input_and_auth_pass()
    {
        var validators = new IValidator<TestCommand>[]
        {
            new InputValidator(shouldFail: false),
            new AuthValidator(shouldFail: false),
            new BusinessValidator(shouldFail: true),
        };
        var behavior = new ValidationBehavior<TestCommand, string>(validators);
        var command = new TestCommand("test");

        var act = async () => await behavior.Handle(command, SuccessHandler, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().Contain(e => e.ErrorCode == "ENTITY_NOT_FOUND");
        exception.Which.Errors.Should().AllSatisfy(e =>
            e.CustomState.Should().Be(ValidatorCategory.Business));
    }
}
