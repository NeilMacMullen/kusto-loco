using OpenAI.Chat;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using KustoLoco.Services;

namespace KustoLoco.AI
{
    public partial class OrchestratorMethods
    {
        #region public async Task<bool> TestAccess(SettingsService objSettings, string GPTModel)
        public async Task<bool> TestAccess(SettingsService objSettings, string GPTModel)
        {
            var chatClient = CreateAIChatClient(objSettings);
            string SystemMessage = "Please return the following as json: \"This is successful\" in this format {\r\n  'message': message\r\n}";
            var response = await chatClient.CompleteAsync(SystemMessage);

            if (response.Choices.Count == 0)
            {
                return false;
            }

            return true;
        }
        #endregion
    }
}
