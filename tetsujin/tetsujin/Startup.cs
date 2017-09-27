using System;
using System.Collections.Generic;
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

namespace tetsujin
{
    public class Startup
    {
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

            var storageAccount = configuration.GetValue<string>("STRORAGE_ACCOUNT");
            var storageKey = configuration.GetValue<string>("STRORAGE_KEY");
            var storageUrl = configuration.GetValue<string>("STRORAGE_URL");
            BlobFile.SetAccountInfo(storageAccount, storageKey, storageUrl);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<HtmlEncoder>(
                HtmlEncoder.Create(allowedRanges: new[] {
                    UnicodeRanges.BasicLatin,
                    UnicodeRanges.CjkSymbolsandPunctuation,
                    UnicodeRanges.Hiragana,
                    UnicodeRanges.Katakana,
                    UnicodeRanges.CjkUnifiedIdeographs
                })
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] =
                        "public,max-age=2592000";
                }
            });
        }
    }
}
