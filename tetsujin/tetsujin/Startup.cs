using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.Net.Http.Headers;
using tetsujin.Models;
using MangoFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace tetsujin
{
    public class Startup
    {
        public static string buildTime;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // DB接続確立
            var dbName = "blog";
            DbConnection.Connect(configuration.GetValue<string>("MONGO_CONNECTION"), dbName);

            // 宣言されたモデルからDBにコレクションを作る
            MongoInitializer.Run(DbConnection.Db, "tetsujin");

            // ユーザパスワードのハッシュキー
            Session.Hashkey = configuration.GetValue<string>("HASHKEY");

            var storageAccount = configuration.GetValue<string>("STORAGE_ACCOUNT");
            var storageKey = configuration.GetValue<string>("STORAGE_KEY");
            var storageUrl = configuration.GetValue<string>("STORAGE_URL");
            BlobFile.SetAccountInfo(storageAccount, storageKey, storageUrl);

            buildTime = configuration.GetValue<string>("BUILD_TIME");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<GzipCompressionProviderOptions>
                (options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options =>
            {
                options.Providers.Add<GzipCompressionProvider>();
            });
            services.AddMvc(options => options.EnableEndpointRouting = false)
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                    .AddNewtonsoftJson();

            services.AddSingleton<HtmlEncoder>(
                HtmlEncoder.Create(allowedRanges: new[] {
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.CjkSymbolsandPunctuation,
                    UnicodeRanges.Hiragana,
                    UnicodeRanges.Katakana,
                    UnicodeRanges.CjkUnifiedIdeographs
                })
            );
            services.AddControllers();
            services.AddRazorPages();

            var githubClientId = Configuration.GetValue<string>("GITHUB_CLIENT_ID");
            var githubClientSecret = Configuration.GetValue<string>("GITHUB_CLIENT_SECRET");
            var githubOAuth = new GithubOAuth(githubClientId, githubClientSecret);
            services.AddSingleton<GithubOAuth>(githubOAuth);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            app.UseResponseCompression();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=2592000";
                }
            });

            app.UseRouting();
            app.UseMvc();
        }
    }
}
