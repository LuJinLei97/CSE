namespace CSE.Classes;
public class Method
{
    public static Method DefaultMethod => new() { Name = "Method" };

    public virtual string Name { get => name ?? (Delegate != null ? $"{Delegate.Method.DeclaringType.FullName}.{Delegate.Method.Name}" : DefaultMethod.Name); set => name = value; }
    protected string name;

    public virtual Delegate Delegate { get; set; }

    public static implicit operator Delegate(Method method) => method?.Delegate;
}
