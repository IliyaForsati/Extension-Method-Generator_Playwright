﻿using MSTestProject.ComponentHelper;
using RTProSL_MSTest.TestClasses;
using SeleniumWebdriver.Settings;

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

    #region Extentions
    /// <summary>
    /// page.GenerateAsync(); -> generate data in page
    /// </summary>
    /// <param name="page"></param>
    /// <param name="selector">select a locator in the page</param>
    /// <param name="value">set the value in page with selector -> for DropDown, Combo & ComboGrid</param>
    /// <param name="mode">select the mode like DropDown or RangeInput from GenerateMode</param>
    /// <exception cref="Exception">with message: can not use this mode for page. call this mode with the proper locator or selector</exception>
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
            case GenerateMode.Combo:
            case GenerateMode.ComboInGrid:
            case GenerateMode.RangeInput:
                throw new Exception(message: "can not use this mode for page. call this mode with the proper locator or selector");
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

        var count = await locator.CountAsync();
        if (count > 1)
            for (var i = 0; i < count; i++)
                await locator.Nth(i).GenerateAsync(selector: selector, value: value, mode: mode);

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
                await DropDownListGeneratorAsync(locator, value);
                break;
            case GenerateMode.Combo:
                await ComboAutoCompleteGenerator(locator, value);
                break;
            case GenerateMode.ComboInGrid:
                await ComboAutoCompleteGeneratorInGrid(locator, value);
                break;
            case GenerateMode.RangeInput:
                await RangeInputGenerator(locator);
                break;
        }
    }
    #endregion

    // helper methods
    /// <summary>
    /// this method is for generate data in a context with several fields in type input, ...
    /// that all have mode = GenerateMode.Default
    /// </summary>
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
                await TextboxGenerator(input);

            else if (type == "email" || inputMode == "email")
                await EmailGenerator(input);

            else if (type == "password")
                await PasswordGenerator(input);

            else if (type == "tel")
                await TellGenerator(input);

            else if (type == "url" || name == "trackingURL" || name == "website")
                await UrlGenerator(input);

            else if (type == "range")
                await RangeInputGenerator(input);

            else if (type == "checkbox")
                await CheckboxGenerator(input);
        }

        // for text areas

        IReadOnlyList<ILocator>? textareas;
        if (page != null) textareas = await page.Locator("textarea:not([disabled])").AllAsync();
        else textareas = await locaror.Locator("textarea:not([disabled])").AllAsync();
        foreach (var textarea in textareas)
            await TextAreaGenerator(textarea);

        // for date picker 
        IReadOnlyList<ILocator>? datePickers;
        if (page != null) datePickers = await page.Locator(".main-modal .main-date-picker_date-wrapper, [data-form-item-type='MainDatePicker'][data-form-item-disabled='false']").AllAsync();
        else datePickers = await locaror.Locator(".main-modal .main-date-picker_date-wrapper, [data-form-item-type='MainDatePicker'][data-form-item-disabled='false']").AllAsync();
        foreach (var datePicker in datePickers)
            await DatePickerGenerator(datePicker);

        // for color picker
        IReadOnlyList<ILocator>? colorPickers;
        if (page != null) colorPickers = await page.Locator("[data-form-item-type='advanceColorPicker']").AllAsync();
        else colorPickers = await locaror.Locator("[data-form-item-type='advanceColorPicker']").AllAsync();
        foreach (var cp in colorPickers)
            await ColorPickerDropDownGenerator(cp);

        // for drop down list
        IReadOnlyList<ILocator>? dropDowns;
        if (page != null) dropDowns = await page.Locator("[data-form-item-type='select'][data-section='formItem']").AllAsync();
        else dropDowns = await locaror.Locator("[data-form-item-type='select'][data-section='formItem']").AllAsync();
        foreach (var dropDown in dropDowns)
            await DropDownListGeneratorAsync(dropDown);

        // for combo  NOT comboInGrid
        IReadOnlyList<ILocator>? combos;
        if (page != null) combos = await page.Locator(".combo-auto-complete").AllAsync();
        else combos = await locaror.Locator(".combo-auto-complete").AllAsync();
        foreach (var combo in combos)
            await ComboAutoCompleteGenerator(combo);
    }

    #region Generators
    private static async Task ComboAutoCompleteGenerator(ILocator combo, string? filter = null)
    {
        try
        {
            var gridIcons = combo.Locator("i[data-icon-name='grid']");
            int gridCount = await gridIcons.CountAsync();

            for (int i = 0; i < gridCount; i++)
            {
                var icon = gridIcons.Nth(i);
                int tryToClickCount = 0;

            tryClick:
                try
                {
                    if (tryToClickCount >= 2)
                        continue;

                    tryToClickCount++;
                    await icon.ClickAsync(new() { Timeout = 3000 });
                }
                catch
                {
                    try
                    {
                        await ObjectRepository.Driver.OriginalPage.EvalOnSelectorAsync(
                            ".tab-content[data-tab-active='true']",
                            "el => el.scrollTo({top:0, left:0})"
                        );
                        await Task.Delay(200);
                        goto tryClick;
                    }
                    catch
                    {
                        continue;
                    }
                }

                await ObjectRepository.Driver.OriginalPage.Locator(".loading-parent").WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = 10000 });

                var rndWrapper = ObjectRepository.Driver.OriginalPage.Locator(".rnd-wrapper");
                if (!await rndWrapper.IsVisibleAsync()) break;

                var comboContext = rndWrapper.Locator(".ag-body-viewport.ag-row-animation.ag-layout-normal");

                if (!string.IsNullOrEmpty(filter))
                {
                    var searchInput = rndWrapper.Locator(".combo-drag-container [placeholder='Search']");
                    await searchInput.FillAsync(filter);
                    await searchInput.PressAsync("Enter");
                    await ObjectRepository.Driver.OriginalPage.WaitForTimeoutAsync(800);

                    var filteredItem = comboContext.Locator("div", new() { HasText = filter });
                    if (await filteredItem.IsVisibleAsync())
                    {
                        await filteredItem.ClickAsync();
                        await rndWrapper.Locator(".more-items-visible-children-wrapper .icon-select-grid").ClickAsync();
                        return;
                    }
                }

                var allItems = comboContext.Locator("div.ag-row");
                int count = await allItems.CountAsync();
                if (count > 0)
                {
                    int randomIndex = int.Parse(RandomValueGenerator.GenerateRandomInt(0, count - 1));
                    await allItems.Nth(randomIndex).ClickAsync();

                    var selectBtn = rndWrapper.Locator(".icon-select-grid");
                    if (await selectBtn.IsVisibleAsync())
                        await selectBtn.ClickAsync();
                }

                await ObjectRepository.Driver.OriginalPage.Locator(".loading-parent").WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = 10000 });

                var confirmDialog = ObjectRepository.Driver.OriginalPage.Locator("[data-section='confirmDialog']");
                if (await confirmDialog.IsVisibleAsync())
                {
                    await ObjectRepository.Driver.OriginalPage.WaitForTimeoutAsync(1000);
                    var bc = new BaseClass();
                    bc.ConfirmBtnCheck(dataSection: "CONFIRM_DIALOG", confirm: false);
                }
            }
        }
        catch
        {
            // ignore
        }
    }

    private static async Task ComboAutoCompleteGeneratorInGrid(ILocator combo, string filter = null)
    {
        try
        {
            int tryCount = 0;

        retryClick:
            if (tryCount >= 2)
                return;

            tryCount++;

            await combo.ClickAsync();

            var page = ObjectRepository.Driver.OriginalPage;
            var wrapper = page.Locator(".rnd-wrapper");

            if (!await wrapper.IsVisibleAsync())
            {
                await combo.ClickAsync();
            }

            await page.WaitForSelectorAsync(".loading-parent", new() { State = WaitForSelectorState.Detached });

            var refreshBtn = page.Locator(".combo-toolbox-container .icon-refresh");
            if (await refreshBtn.IsVisibleAsync())
            {
                await refreshBtn.Last.ClickAsync();
            }

            await page.WaitForSelectorAsync(".loading-parent", new() { State = WaitForSelectorState.Detached });

            if (!string.IsNullOrEmpty(filter))
            {
                var searchBox = page.Locator(".top-actions-combo-grid [placeholder='Search']");
                await searchBox.FillAsync(filter);
                await page.WaitForTimeoutAsync(500);

                var itemList = wrapper.Locator(".ag-body-viewport.ag-row-animation.ag-layout-normal div[role='row']");
                var firstItem = itemList.First;
                if (await firstItem.IsVisibleAsync())
                {
                    await firstItem.ClickAsync();
                }

                var confirmDialog = page.Locator("[data-section='confirmDialog']");
                if (await confirmDialog.IsVisibleAsync(new() { Timeout = 2000 }))
                {
                    var cancelBtn = confirmDialog.Locator("button:has-text('No'), button:has-text('Cancel')");
                    if (await cancelBtn.IsVisibleAsync())
                    {
                        await cancelBtn.ClickAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Generator.ComboAutoCompleteGeneratorInGrid: " + ex.Message);
        }
    }


    private static async Task DropDownListGeneratorAsync(ILocator dropDown)
    {
        try
        {
        tryToClickOption:
            await dropDown.ClickAsync();
            await Task.Delay(300);

            var page = dropDown.Page;
            var options = page.Locator(".ant-select-item.ant-select-item-option");
            int count = await options.CountAsync();

            if (count <= 1) return;

            int index = int.Parse(RandomValueGenerator.GenerateRandomInt(1, count - 1));

            try
            {
                await options.Nth(index).ClickAsync();
            }
            catch
            {
                await Task.Delay(200);
                goto tryToClickOption;
            }

            var cancelModal = page.Locator("[data-modal-title='CANCEL_REASON']");
            if (await cancelModal.IsVisibleAsync())
            {
                var bc = new BaseClass();
                bc.ConfirmBtnCheck(dataSection: "MODAL", confirm: false);
            }
            
        }
        catch
        {
            // ignored
        }
    }
    private static async Task DropDownListGeneratorAsync(ILocator dropDown, string value)
    {
        await dropDown.ClickAsync();
        await Task.Delay(250);

        var option = ObjectRepository.Driver.OriginalPage.Locator($"[title='{value}'] > .ant-select-item-option-content");
        await option.ClickAsync();
    }

    private static async Task ColorPickerDropDownGenerator(ILocator input)
    {
        var colorTrigger = input.Locator(".main-color-picker-dropDown_wrapper");
        await colorTrigger.ClickAsync();

        var colorContext = input.Page.Locator(".main-color-picker-popover");
        var colorList = colorContext.Locator(".color-item-wrapper");

        int count = await colorList.CountAsync();
        if (count > 0)
        {
            var randomIndex = RandomValueGenerator.GenerateRandomInt(0, count - 1);
            await colorList.Nth(int.Parse(randomIndex)).ClickAsync();
        }
    }

    private static async Task DatePickerGenerator(ILocator input)
    {
        var dateTrigger = input.Locator(".ant-picker-input");
        await dateTrigger.ClickAsync();

        var todayBtn = input.Page.Locator(".ant-picker-today-btn");
        var nowBtn = input.Page.Locator(".ant-picker-now-btn");

        if (await todayBtn.IsVisibleAsync())
        {
            await todayBtn.Last.ClickAsync();
        }
        else if (await nowBtn.IsVisibleAsync())
        {
            await nowBtn.Last.ClickAsync();
        }
    }

    private static async Task TextAreaGenerator(ILocator textarea)
    {
        var value = RandomValueGenerator.GenerateTextArea(14);
        await textarea.FillAsync("");
        await textarea.FillAsync(value);
    }

    private static async Task CheckboxGenerator(ILocator input)
    {
        await input.ClearAsync();
    }

    private static async Task RangeInputGenerator(ILocator input)
    {
        var valueAttr = await input.GetAttributeAsync("value");
        var maxAttr = await input.GetAttributeAsync("max");

        if (float.TryParse(valueAttr, out var currentVal) && float.TryParse(maxAttr, out var maxVal))
        {
            var steps = int.Parse(RandomValueGenerator.GenerateRandomInt((int)currentVal, (int)maxVal));
            for (int i = 0; i < steps; i++)
            {
                await input.PressAsync("ArrowRight");
            }
        }
    }

    private static async Task UrlGenerator(ILocator input)
    {
        var value = "https://" + RandomValueGenerator.GenerateRandomString(14) + ".com";
        await input.FillAsync("");
        await input.FillAsync(value);
    }

    private static async Task TellGenerator(ILocator input)
    {
        var maxLengthAttr = await input.GetAttributeAsync("maxlength");
        int maxLength = 11;

        if (!string.IsNullOrEmpty(maxLengthAttr))
        {
            if (int.TryParse(maxLengthAttr, out var parsed))
                maxLength = parsed;
        }

        var value = "09" + RandomValueGenerator.GenerateRandomInt(maxLength - 2);
        await input.FillAsync("");
        await input.FillAsync(value);
    }

    private static async Task PasswordGenerator(ILocator input)
    {
        var maxLengthAttr = await input.GetAttributeAsync("maxlength");
        int maxLength = 15;

        if (!string.IsNullOrEmpty(maxLengthAttr))
        {
            if (int.TryParse(maxLengthAttr, out var parsedMax))
            {
                maxLength = Math.Min(parsedMax, 15);
            }
        }

        var value = RandomValueGenerator.GenerateRandomString(maxLength);

        await input.FillAsync("");         
        await input.ClickAsync();          
        await input.FillAsync(value);
    }

    private static async Task EmailGenerator(ILocator input)
    {
        var value = RandomValueGenerator.GenerateGmail();
        await input.FillAsync("");         
        await input.ClickAsync();          
        await input.FillAsync(value);
    }

    private static async Task TextboxGenerator(ILocator input) // remove try/catch ???
    {
        try
        {
            var inputMode = await input.GetAttributeAsync("inputmode");
            var type = await input.GetAttributeAsync("type");
            var maxlengthAttr = await input.GetAttributeAsync("maxlength");

            string value = string.Empty;

            if (inputMode is "numeric" or "decimal" || type is "number")
            {
                int maxlength = 2;
                if (int.TryParse(maxlengthAttr, out var maxLengthParsed))
                    maxlength = Math.Min(maxLengthParsed, 2);

                int min = 0;
                int max = 50;

                var minAttr = await input.GetAttributeAsync("min");
                var maxAttr = await input.GetAttributeAsync("max");

                if (decimal.TryParse(minAttr, out var parsedMin))
                    min = (int)Math.Max(parsedMin, -100);

                if (decimal.TryParse(maxAttr, out var parsedMax))
                    max = (int)Math.Min(parsedMax, 50);

                value = RandomValueGenerator.GenerateRandomInt(min, max);
            }
            else if (inputMode is "text" || type is "text" || string.IsNullOrEmpty(inputMode))
            {
                value = RandomValueGenerator.GenerateRandomString(10);
            }

            await input.FillAsync("");
            await input.ClickAsync();
            await input.FillAsync(value);
        }
        catch
        {
            // ignored
        }
    }
    #endregion
}