using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace RTProSL_MSTest.ComponentHelper;


/// <summary>
///  this class impliment "Extension Method" for IPage and ILocator in "Microsoft.Playwright"
/// </summary>
public static class Generator
{
    // generate mode
    public enum GenerateMode
    {
        Default,
        DropDown,
        Combo,
        ComboInGrid,
        RangeInput,
    }

    // services: 
    /// <summary>
    /// page.GenerateAsync(); -> generate data in page
    /// </summary>
    /// <param name="page"></param>
    /// <param name="selector">select a locator in the page</param>
    /// <param name="value">set the value in page with selector -> for DropDown, Combo & ComboGrid</param>
    /// <param name="mode">select the mode like DropDown or RangeInput from GenerateMode</param>
    public static async Task GenerateAsync(this IPage page, string selector = null,
        string value = null, GenerateMode mode = GenerateMode.Default)
    {

    }
    /// <summary>
    /// locator.GenerateAsync(); -> generate data in locator
    /// </summary>
    /// <param name="locator"></param>
    /// <param name="selector">select a locator in the main locator</param>
    /// <param name="value">set the value in locator -> for DropDown, Combo & ComboGrid</param>
    /// <param name="mode">select the mode like DropDown or RangeInput from GenerateMode</param>
    public static async Task GenerateAsync(this ILocator locator, string selector = null,
        string value = null, GenerateMode mode = GenerateMode.Default)
    {

    }


    // helper methods
}
