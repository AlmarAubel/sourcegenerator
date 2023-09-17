namespace SourceGenerator.core;

public interface IRequest {}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
sealed class GenerateProxyAttribute : Attribute { }