using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocelot.Middleware;
using Ocelot.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using GatewayApi.Aggregators;
using Ocelot.Middleware.Multiplexer;

namespace GatewayApi
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllers();

      services.AddOcelot(Configuration).AddSingletonDefinedAggregator<Aggregator>(); ;

      // var identityUrl = Configuration.GetValue<string>("http://localhost:7003");
      // var authenticationProviderKey = "4kadve15xfgs565hvdly53dsafw";

      // services.AddAuthentication()
      //           .AddJwtBearer(authenticationProviderKey, x =>
      //           {
      //             x.Authority = identityUrl;
      //             x.RequireHttpsMetadata = false;
      //             x.TokenValidationParameters = new TokenValidationParameters()
      //             {
      //               ValidAudiences = new[] { "http://localhost:7001", "http://localhost:7001" }
      //             };
      //           });

      var key = Encoding.ASCII.GetBytes(Configuration["Token:Key"]);

      var audiences = Configuration.GetSection("Token:Audience").Get<List<string>>();

      services.AddAuthentication(x =>
        {
          x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer("ApiSecurity", x =>
        {
          x.RequireHttpsMetadata = false;
          x.SaveToken = true;
          x.TokenValidationParameters = new TokenValidationParameters
          {
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = Configuration["Token:Issuer"],
            ValidateAudience = true,
            ValidAudiences = audiences,
            ClockSkew = TimeSpan.Zero
          };

        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });

      await app.UseOcelot();
    }
  }
}
