using System;

namespace BankApp.Web.Infrastructure;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class AllowAnonymousSessionAttribute : Attribute
{
}
