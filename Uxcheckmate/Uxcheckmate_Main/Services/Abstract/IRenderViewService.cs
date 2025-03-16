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

public interface IViewRenderService
{
    Task<string> RenderViewToStringAsync<TModel>(Controller controller, string viewName, TModel model);
}
