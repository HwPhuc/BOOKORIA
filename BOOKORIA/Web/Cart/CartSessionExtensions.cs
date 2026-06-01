using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace BOOKORIA.Web.Cart;

public static class CartSessionExtensions
{
    private const string CartKey = "BOOKORIA_CART";

    public static List<CartItem> GetCart(this ISession session)
    {
        var json = session.GetString(CartKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? [];
    }

    public static void SaveCart(this ISession session, List<CartItem> cart)
    {
        var json = JsonSerializer.Serialize(cart);
        session.SetString(CartKey, json);
    }

    public static void ClearCart(this ISession session)
    {
        session.Remove(CartKey);
    }
}
