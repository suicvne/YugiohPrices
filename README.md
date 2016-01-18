# YugiohPrices
A C# based wrapper for the yugiohprices.com API.

# Dependencies
`Newtonsoft.Json` >= 8.0

`.Net Framework` >= 4.5

# Quick How-To

This API utilizes `http://yugiohprices.com/` database. It's great. If that's down, this won't work. The searcher supports async. So, if you really want to, you can `await` all day everyday. 

This simple example retrieves a card from their database:
```csharp
YugiohPricesSearcher searcher = new YugiohPricesSearcher(); //the class that will allow us to search
Card c = await searcher.GetCardByName("Dark Magician"); //retrieves a card from the database by name
Console.WriteLine(c.Description); //writes the description or effect to the console
```

```batch
The ultimate wizard in terms of attack and defense.
```

## Retrieving Card Price

This simple example retrieves the first card price average from their database:

```csharp
YugiohPricesSearcher searcher = new YugiohPricesSearcher();
CardPrices prices = await searcher.GetCardPricesByName("Dark Magician"); //alternatively, you can get an array of CardPrices by using GetAllCardPricesByName
Console.WriteLine($"Average price for {prices.Card.Name} in set '{prices.SetName}'"); //look i even give you the card object so you don't have to call GetCardByName again
Console.WriteLine($"- {prices.PriceData.AveragePrice.ToString("C")}");
```

```batch
Average price for Dark Magician in set 'Structure Deck: Spellcaster's Judgment'
- $1.59
```

## i dont wanna use async

You can use this library without `async`. Omit the `await` keyword and append `.Result` to the end of the method, like so:

```csharp
Card c = searcher.GetCardByName("Dark Magician").Result;
```

The result is the same. However, the method will not be run asynchronously and will block your UI thread.

# Known Limitations

Because `yugiohprices.com` is very finicky about casing and symbols and what not, you probably need to enter the card name exactly as it is seen on the card. For example,

`Dark Magician` :100:

`dark magician` :x:

`Black Luster Soldier - Envoy of the Beginning` :100:

`black luster soldier envoy of the beginning` :x:
