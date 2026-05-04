// <copyright file="PagedResult.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace BankApp.Models.DTOs;

using System.Collections.Generic;

/// <summary>
/// Represents a paginated collection response.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public class PagedResult<T>
{
    /// <summary>Gets or sets the current page items.</summary>
    public List<T> Items { get; set; } = new List<T>();

    /// <summary>Gets or sets the total number of available items.</summary>
    public int TotalCount { get; set; }

    /// <summary>Gets or sets the current page number (one-based).</summary>
    public int Page { get; set; }

    /// <summary>Gets or sets the requested page size.</summary>
    public int PageSize { get; set; }
}
