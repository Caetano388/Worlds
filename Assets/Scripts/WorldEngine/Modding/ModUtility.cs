﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// Utility class containing all regular expressions used to parse mod expressions
/// </summary>
public static class ModUtility
{
    public const string IdentifierRegexPart = @"[a-zA-Z_][a-zA-Z0-9_]*";
    public const string NumberRegexPart = @"-?\d+(?:\.\d+)?";
    public const string BooleanRegexPart = @"(true|false)";

    public const string OperatorRegexPart = @"[\!\+\-\*\/\<\>\=]=?";
    public const string AccessorRegexPart = @"\.";

    /// <summary>
    /// Regex used to capture base elements like numbers, booleans and identifiers
    /// </summary>
    public const string BaseStatementRegexPart = 
        @"(?<number>" + NumberRegexPart +
        @")|(?<boolean>" + BooleanRegexPart +
        @")|(?<identifierStatement>" + IdentifierStatementRegexPart +
        @")";

    /// <summary>
    /// Regex used to indentify statements enclosed within parenthesis
    /// </summary>
    public const string InnerStatementRegexPart =
        @"(?:" +
            @"(?:" +
                @"(?<open>\()" +
            @"|" +
                @"(?<innerStatement-open>\))" +
            @")" +
        @"[^\(\)]*?)+" +
        @"(?(open)(?!))";

    /// <summary>
    /// Regex used to indentify a set of argument statements given to a function
    /// </summary>
    public const string ArgumentsRegexPart =
        @"(?:" +
            @"(?:" +
                @"(?<open>\()" +
            @"|" +
                @"(?<arguments-open>\))" +
            @")" +
        @"[^\(\)]*?)+" +
        @"(?(open)(?!))";

    /// <summary>
    /// Regex used to indetify the first argument within a set of arguments (used recursively)
    /// </summary>
    public const string ArgumentListRegex =
        @"^\s*" +
        @"(?<argument>" + ArgumentRegexPart + @")\s*" +
        @"(?:," +
            @"(?<otherArgs>" +
                @".*" +
            @")" +
        @")?$";

    /// <summary>
    /// Regex used to indetify a single valid argument statement
    /// </summary>
    public const string ArgumentRegexPart =
        @"(?<unaryOpStatement>" +
            UnaryOpStatementRegexPart +
        @")|(?<binaryOpStatement>" +
            BinaryOpStatementRegexPart +
        @")|" +
        OperandStatementRegexPart;

    /// <summary>
    /// Regex used to identify an operation's operand statements
    /// </summary>
    public const string OperandStatementRegexPart =
        @"(?<accessorOpStatement>" +
            AccessorOpStatementRegexPart +
        @")|(?<baseStatement>" +
            BaseStatementRegexPart +
        @")|(?<innerStatement>" +
            InnerStatementRegexPart +
        @")";

    /// <summary>
    /// Regex used to indetify an accessible statement or entity
    /// </summary>
    public const string AccessibleStatementRegexPart =
        @"(?<identifierStatement>" +
            IdentifierStatementRegexPart +
        @")|(?<innerStatement>" +
            InnerStatementRegexPart +
        @")";

    /// <summary>
    /// Regex used to indentify an identifier (and it's possible arguments)
    /// </summary>
    public const string IdentifierStatementRegexPart =
        @"(?<identifier>" + IdentifierRegexPart + @")\s*" +
        @"(?:" + ArgumentsRegexPart + @")?";

    /// <summary>
    /// Regex used to indentify a identifier (bounded)
    /// </summary>
    public const string IdentifierStatementRegex =
        @"^\s*" + IdentifierStatementRegexPart + @"\s*$";

    /// <summary>
    /// Regex used to indentify an unary operation
    /// </summary>
    public const string UnaryOpStatementRegexPart =
        @"(?<unaryOp>" + OperatorRegexPart + @")" +
        @"(?<statement>" + OperandStatementRegexPart + @")";

    /// <summary>
    /// Regex used to indentify an unary operation (bounded)
    /// </summary>
    public const string UnaryOpStatementRegex =
        @"^\s*" + UnaryOpStatementRegexPart + @"\s*$";

    /// <summary>
    /// Regex used to indentify a binary operation
    /// </summary>
    public const string BinaryOpStatementRegexPart =
        @"(?<statement1>" + OperandStatementRegexPart + @")\s*" +
        @"(?<binaryOp>" + OperatorRegexPart + @")\s*" +
        @"(?<statement2>" +
            @"(?<operand2>" + OperandStatementRegexPart + @")\s*" +
            @"(?<restOp>" +
                OperatorRegexPart + @"\s*" +
                @"(?:" + OperandStatementRegexPart + @")" +
            @")*" +
        @")";

    /// <summary>
    /// Regex used to indentify a binary operation (bounded)
    /// </summary>
    public const string BinaryOpStatementRegex =
        @"^\s*" + BinaryOpStatementRegexPart + @"\s*$";

    /// <summary>
    /// Regex used to indentify an access operation
    /// </summary>
    public const string AccessorOpStatementRegexPart =
        @"(?<statement>" +
            @"(?:" + AccessibleStatementRegexPart + @")" +
            @"(?:" +
                AccessorRegexPart +
                IdentifierStatementRegexPart +
            @")*" +
        @")" +
        AccessorRegexPart +
        @"(?<attribute>" + IdentifierStatementRegexPart + @")";

    /// <summary>
    /// Regex used to indentify an access operation (bounded)
    /// </summary>
    public const string AccessorOpStatementRegex =
        @"^\s*" + AccessorOpStatementRegexPart + @"\s*$";

    /// <summary>
    /// Regex used to capture base elements (bounded)
    /// </summary>
    public const string BaseStatementRegex =
        @"^\s*(?<statement>" + BaseStatementRegexPart + @")\s*$";
    /// <summary>
    /// Regex used to capture an operation operands (bounded)
    /// </summary>
    public const string OperandStatementRegex =
        @"^\s*(?<statement>" + OperandStatementRegexPart + @")\s*$";
    /// <summary>
    /// Regex used to capture an statement enclosed within parenthesis (bounded)
    /// </summary>
    public const string InnerStatementRegex =
        @"^\s*(?<statement>" + InnerStatementRegexPart + @")\s*$";
    /// <summary>
    /// Regex used to indetify an accessible statement or entity (bounded)
    /// </summary>
    public const string AccessibleStatementRegex =
        @"^\s*(?<statement>" + AccessibleStatementRegexPart + @")\s*$";

#if DEBUG
    /// <summary>
    /// Print a regex group for debugging purposes
    /// </summary>
    /// <param name="group">group to print</param>
    /// <returns></returns>
    public static string Debug_CapturesToString(Group group)
    {
        Capture[] captures = new Capture[group.Captures.Count];
        group.Captures.CopyTo(captures, 0);

        return string.Join("; ", captures.Select(c => c.Value));
    }
#endif
}
