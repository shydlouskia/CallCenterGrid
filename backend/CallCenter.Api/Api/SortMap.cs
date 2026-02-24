using CallCenter.Api.Models;
using System.Linq.Expressions;

namespace CallCenter.Api.Api;

public static class SortMap
{
    public static readonly Dictionary<string, Expression<Func<Contact, object>>> Map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["id"] = x => x.Id,
            ["firstName"] = x => x.FirstName,
            ["lastName"] = x => x.LastName,
            ["email"] = x => x.Email,
            ["phone"] = x => x.Phone,
            ["city"] = x => x.City,
            ["state"] = x => x.State,
            ["age"] = x => x.Age,
            ["status"] = x => x.Status
        };
}
