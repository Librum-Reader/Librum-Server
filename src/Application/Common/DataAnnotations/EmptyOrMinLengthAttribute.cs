using System.ComponentModel.DataAnnotations;
using Microsoft.IdentityModel.Tokens;

namespace Application.Common.DataAnnotations;

/// Specifies the minimum length of string data, which is still allowed to be
/// empty, in an attribute.
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | 
                AttributeTargets.Parameter, AllowMultiple = false)]
public class EmptyOrMinLengthAttribute : ValidationAttribute
{
    public int Length { get; private set; }
    
    public EmptyOrMinLengthAttribute(int length) : base("String is smaller than the " + 
                                                        "minimum length.")
    {
        Length = length;
    }
    
    public override bool IsValid(object value)
    {
        if (Length < 0)
            throw new InvalidOperationException("Minimum length can not be less than 0.");

        var str = value as string;
        if (str.IsNullOrEmpty()) // Automatically pass if value is null or empty
        {
            return true;
        }

        return str!.Length >= Length;
    }
}