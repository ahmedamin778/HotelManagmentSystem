using System;
using System.Collections.Generic;
using System.Text;

namespace HotelBookingSystem.Application.DTOs.Common;

public class PaginatedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}