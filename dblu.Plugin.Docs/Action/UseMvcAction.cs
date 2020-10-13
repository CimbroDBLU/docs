using System;
using ExtCore.Mvc.Infrastructure.Actions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace dblu.Portale.Plugin.Documenti.Action
{
    public class UseMvcAction : IUseEndpointsAction// IUseMvcAction
    {
        public int Priority => 1000;

        public void Execute(IEndpointRouteBuilder endpointRouteBuilder, IServiceProvider serviceProvider)
        {
            //throw new NotImplementedException();
        }
    }
}
