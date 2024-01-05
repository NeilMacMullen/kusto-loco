public static class VegaMaker
{
    public static string MakeHtml(string title,string spec)
    {
        var header = $$"""
                              <!doctype html>
                              <html>
                              <style>
                              .fullscreen {
                                width: 100vw;
                                height: 90vh;
                              }
                              </style>
                                <head>
                                  <title>{{title}}</title>
                                  <script src="https://cdn.jsdelivr.net/npm/vega@5.25.0"></script>
                                  <script src="https://cdn.jsdelivr.net/npm/vega-lite@5.16.3"></script>
                                  <script src="https://cdn.jsdelivr.net/npm/vega-embed@6.22.2"></script>
                                </head>
                                <body>
                                  <div id="vis" class="fullscreen"></div>
                              
                                  <script type="text/javascript">
                                 var yourVlSpec =
                              """;
        const string footer = """
                                   ;
                                    vegaEmbed('#vis', yourVlSpec,{theme: 'carbong100'});
                                  </script>
                                </body>
                              </html>
                              """;
        return header + spec + footer;
    }
}