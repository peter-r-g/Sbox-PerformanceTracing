using System;

namespace PerformanceTracing;

/// <summary>
/// Represents a point to a referenced piece of text.
/// </summary>
public readonly struct SourceLocation : IEquatable<SourceLocation>
{
	/// <summary>
	/// Represents no referenced location.
	/// </summary>
	public static readonly SourceLocation None = new( "unknown.cs", -1 );

	/// <summary>
	/// The absolute file path to the referenced location.
	/// </summary>
	public string FilePath { get; }
	/// <summary>
	/// The line number inside the <see cref="FilePath"/> to the referenced location.
	/// </summary>
	public int LineNumber { get; }

	/// <summary>
	/// Initializes a new instance of <see cref="SourceLocation"/>.
	/// </summary>
	/// <param name="filePath">The absolute file path to the referenced location.</param>
	/// <param name="lineNumber">The line number inside the <paramref name="filePath"/> to the referenced location.</param>
	public SourceLocation( string filePath, int lineNumber )
	{
		FilePath = filePath;
		LineNumber = lineNumber;
	}

	/// <inheritdoc/>
	public override bool Equals( object? obj )
	{
		return obj is SourceLocation other && Equals( other );
	}

	/// <inheritdoc/>
	public bool Equals( SourceLocation other )
	{
		return FilePath == other.FilePath && LineNumber == other.LineNumber;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return HashCode.Combine( FilePath, LineNumber );
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return FilePath + ':' + LineNumber;
	}

	/// <inheritdoc/>
	public static bool operator ==( SourceLocation left, SourceLocation right ) => left.Equals( right );
	/// <inheritdoc/>
	public static bool operator !=( SourceLocation left, SourceLocation right ) => !(left == right);
}
