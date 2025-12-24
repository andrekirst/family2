using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace FamilyHub.Tests.Unit;

/// <summary>
/// Custom AutoData attribute that configures AutoFixture with AutoNSubstitute customization.
/// This enables automatic creation of NSubstitute mocks for interface dependencies in test methods.
/// </summary>
/// <example>
/// <code>
/// [Theory, AutoNSubstituteData]
/// public async Task MyTest(
///     IUserRepository userRepository,  // Auto-mocked by NSubstitute
///     IFamilyRepository familyRepository,  // Auto-mocked by NSubstitute
///     CreateFamilyCommandHandler sut)  // Auto-constructed with mocked dependencies
/// {
///     // Arrange
///     userRepository.GetByIdAsync(Arg.Any&lt;UserId&gt;(), Arg.Any&lt;CancellationToken&gt;())
///         .Returns(someUser);
///
///     // Act
///     var result = await sut.Handle(command, CancellationToken.None);
///
///     // Assert
///     await userRepository.Received(1).GetByIdAsync(userId, Arg.Any&lt;CancellationToken&gt;());
/// }
/// </code>
/// </example>
public class AutoNSubstituteDataAttribute() : AutoDataAttribute(() => new Fixture().Customize(new AutoNSubstituteCustomization()));
