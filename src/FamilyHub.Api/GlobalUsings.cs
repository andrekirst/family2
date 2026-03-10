// Import our application namespace globally so Result<T> resolves to ours.
// GreenDonut's Result<T> is excluded via <Using Remove="GreenDonut" /> in .csproj.
global using FamilyHub.Common.Application;

// Hot Chocolate type extensions (UsePaging, UseFiltering, etc.)
global using HotChocolate.Types;
