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
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        if (selector != null)
        {
            var locator = page.Locator(selector);
            await locator.GenerateAsync(value: value, mode: mode);
            return;
        }

        switch (mode)
        {
            case GenerateMode.Default:
                await DefaultGenerate(page: page);
                break;
            case GenerateMode.DropDown:
                break;
            case GenerateMode.Combo:
                break;
            case GenerateMode.ComboInGrid:
                break;
            case GenerateMode.RangeInput:
                break;
        }
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
        if (locator == null)
            throw new ArgumentNullException(nameof(locator));

        if (selector != null)
        {
            var locator2 = locator.Locator(selector);
            await locator2.GenerateAsync(value: value, mode: mode);
            return;
        }

        switch (mode)
        {
            case GenerateMode.Default:
                await DefaultGenerate(locaror: locator);
                break;
            case GenerateMode.DropDown:
                break;
            case GenerateMode.Combo:
                break;
            case GenerateMode.ComboInGrid:
                break;
            case GenerateMode.RangeInput:
                break;
        }
    }


    // helper methods
    private static async Task DefaultGenerate(IPage page = null, ILocator locaror = null)
    {
        if (page == null && locaror == null)
            throw new ArgumentNullException(nameof(page));

        // know we want to find element in page or locator and set a random value in them
        // for inputs
        string inputSelector = "input:not(.main-input-read-only):not(.custom-disabled-checkbox):not([disabled])";
        ILocator? inputsLocator;
        if (page != null) inputsLocator = page.Locator(inputSelector);
        else inputsLocator = locaror.Locator(inputSelector);

        var count = await inputsLocator.CountAsync();
        for (var i = 0; i < count; i++)
        {
            var input = inputsLocator.Nth(i);

            var type = (await input.GetAttributeAsync("type")) ?? "text";
            var name = await input.GetAttributeAsync("name") ?? "";
            var placeholder = await input.GetAttributeAsync("placeholder") ?? "";
            var inputMode = await input.GetAttributeAsync("inputmode");

            if (type == "text" && placeholder != "Search Code" && name != "upload" && type != "file")
                TextboxGenerator(input);

            else if (type == "email" || inputMode == "email")
                EmailGenerator(input);

            else if (type == "password")
                PasswordGenerator(input);

            else if (type == "tel")
                TellGenerator(input);

            else if (type == "url" || name == "trackingURL" || name == "website")
                UrlGenerator(input);

            else if (type == "range")
                RangeInputGenerator(input);

            else if (type == "checkbox")
                CheckboxGenerator(input);
        }

        // for text areas

        IReadOnlyList<ILocator>? textareas;
        if (page != null) textareas = await page.Locator("textarea:not([disabled])").AllAsync();
        else textareas = await locaror.Locator("textarea:not([disabled])").AllAsync();
        foreach (var textarea in textareas)
            TextAreaGenerator(textarea);

        // for date picker 
        IReadOnlyList<ILocator>? datePickers;
        if (page != null) datePickers = await page.Locator(".main-modal .main-date-picker_date-wrapper, [data-form-item-type='MainDatePicker'][data-form-item-disabled='false']").AllAsync();
        else datePickers = await locaror.Locator(".main-modal .main-date-picker_date-wrapper, [data-form-item-type='MainDatePicker'][data-form-item-disabled='false']").AllAsync();
        foreach (var datePicker in datePickers)
            DatePickerGenerator(datePicker);

        // for color picker
        IReadOnlyList<ILocator>? colorPickers;
        if (page != null) colorPickers = await page.Locator("[data-form-item-type='advanceColorPicker']").AllAsync();
        else colorPickers = await locaror.Locator("[data-form-item-type='advanceColorPicker']").AllAsync();
        foreach (var cp in colorPickers)
            ColorPickerDropDownGenerator(cp);

        // for drop down list
        IReadOnlyList<ILocator>? dropDowns;
        if (page != null) dropDowns = await page.Locator("[data-form-item-type='select'][data-section='formItem']").AllAsync();
        else dropDowns = await locaror.Locator("[data-form-item-type='select'][data-section='formItem']").AllAsync();
        foreach (var dropDown in dropDowns)
            DropDownListGenerator(dropDown);

        // for combo  NOT comboInGrid
        IReadOnlyList<ILocator>? combos;
        if (page != null) combos = await page.Locator(".combo-auto-complete").AllAsync();
        else combos = await locaror.Locator(".combo-auto-complete").AllAsync();
        foreach (var combo in combos)
            ComboAutoCompleteGenerator(combo);
    }
}