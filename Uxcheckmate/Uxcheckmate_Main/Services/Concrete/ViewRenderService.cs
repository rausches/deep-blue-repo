using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Uxcheckmate_Main.Models;
using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Services;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

public class ViewRenderService : IViewRenderService
{
    public async Task<string> RenderViewToStringAsync<TModel>(Controller controller, string viewName, TModel model)
    {
        // Assign the model to the ViewData to ensure the view has access to it.
        controller.ViewData.Model = model;

        // Create a StringWriter to capture the rendered HTML output.
        using var writer = new StringWriter();

        // Retrieve the view engine service from the dependency injection container.
        var viewEngine = controller.HttpContext.RequestServices.GetService<ICompositeViewEngine>();

        // Attempt to find the specified view using the view engine.
        var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

        // If the view cannot be found, throw an exception.
        if (!viewResult.Success)
            throw new Exception($"View '{viewName}' not found");

        // Create a ViewContext, which encapsulates all information required for rendering.
        var viewContext = new ViewContext(
            controller.ControllerContext, // Current controller context
            viewResult.View,              // The view to render
            controller.ViewData,          // ViewData containing the model
            controller.TempData,          // Temporary data storage
            writer,                       // Output writer to capture the rendered view
            new HtmlHelperOptions()       // Helper options for rendering
        );

        // Render the view asynchronously into the StringWriter.
        await viewResult.View.RenderAsync(viewContext);

        // Return the rendered content as a string.
        return writer.ToString();
    }
}