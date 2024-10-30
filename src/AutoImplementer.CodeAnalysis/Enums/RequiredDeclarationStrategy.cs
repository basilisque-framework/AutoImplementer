using System;
using System.Collections.Generic;
using System.Text;

namespace Basilisque.AutoImplementer.Enums;

/*
 * I know you're probably asking yourself, "Why are these not actually enums??"
 * 
 * The reason is that user code and analyzer code are compiled separately, so any form of
 * passing a proper enum from one to the other is likely impossible.
 */

/// <summary>
/// Methods of deciding if an auto-implemented property should be marked as 'required' or not.
/// </summary>
public class RequiredDeclarationStrategy {
    public const int None = 0;
    public const int NonNullable = 1;
    public const int All = 2;
}
