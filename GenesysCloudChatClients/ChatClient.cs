using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Extensions;
using PureCloudPlatform.Client.V2.Model;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows;

namespace GenesysCloudChatClients
{
    public class ChatClient
    {
        private string organizationId = ""; // YOUR ORGANIZATION ID
        private string deploymentId = ""; // YOUR DEPLOYMENT ID
        private string queueName = "Logitravel_Bookings";
        private string conversationId = String.Empty;
        private string jwt = string.Empty;
        public string ConversationId {
            get {
                return conversationId;
            }
            set {
                conversationId = value;
            }
        }

        private string memberId = String.Empty;
        private string bearerToken = String.Empty;
        public event EventHandler ConversationStarted;

        private void SendEventStarted() {
            OnEvent();
        }

        private void OnEvent()
        {
            if (ConversationStarted != null)
            {
                ConversationStarted(this, EventArgs.Empty);
            }
        }
        private WebChatApi apiInstance = null;
        public ChatClient()
        {
            this.apiInstance = new WebChatApi();
        }

        private void SetJWT() {
            string key = "Authorization";
            string val;
            if (Configuration.Default.ApiKey.TryGetValue(key, out val))
            {
                // yay, value exists!
                Configuration.Default.ApiKey[key] = "Bearer " + this.jwt;
            }
            else
            {
                // darn, lets add the value
                Configuration.Default.ApiKey.Add(key, "Bearer " + this.jwt);
            }
        }

        public void SendMessage(string message)
        {
            SetJWT();
            CreateWebChatMessageRequest msgRequest = new CreateWebChatMessageRequest();
            msgRequest.Body = message;
            msgRequest.BodyType = CreateWebChatMessageRequest.BodyTypeEnum.Standard;
            apiInstance.PostWebchatGuestConversationMemberMessages(conversationId, memberId, msgRequest);
        }

        public void EndChat()
        {
            apiInstance.DeleteWebchatGuestConversationMember(conversationId, memberId);

        }

        private void ProcessMessage(Dictionary<string, object> notification)
        {
            object eventBody, bodyType, body, sendr;
            if (notification.TryGetValue("eventBody", out eventBody))
            {
                Dictionary<string, object> eBody = ((JObject)eventBody).ToObject<Dictionary<string, object>>();
                if (eBody.TryGetValue("bodyType", out bodyType) &&
            eBody.TryGetValue("body", out body) &&
            eBody.TryGetValue("sender", out sendr))
                {
                    Dictionary<string, object> senderOb = ((JObject)sendr).ToObject<Dictionary<string, object>>();
                    object senderId;

                    if (bodyType.ToString().Equals("standard") &&
                    senderOb.TryGetValue("id", out senderId) && !senderId.ToString().Equals(memberId))
                    {
                        Console.WriteLine("Chat: "+ conversationId+ " Agent: " + body.ToString());
                    }
                }
            }
        }

        public void CreateChat(string displayName, string firstName, string lastName, Dictionary<string,string> kvps)
        {

            PureCloudRegionHosts region = PureCloudRegionHosts.eu_west_1;
            Configuration.Default.ApiClient.setBasePath(region);

            CreateWebChatConversationRequest request = new CreateWebChatConversationRequest()
            {
                OrganizationId = this.organizationId,
                DeploymentId = this.deploymentId,
                RoutingTarget = new WebChatRoutingTarget()
                {
                    TargetType = WebChatRoutingTarget.TargetTypeEnum.Queue,
                    TargetAddress = this.queueName
                },
                MemberInfo = new GuestMemberInfo()
                {
                    DisplayName = displayName,
                    CustomFields = kvps
                }
            };


            try
            {
                // Create an ACD chat conversation from an external customer.
                CreateWebChatConversationResponse result = apiInstance.PostWebchatGuestConversations(request);
                this.conversationId = result.Id;
                this.memberId = result.Member.Id;
                if (!string.IsNullOrEmpty(conversationId)) {
                    SendEventStarted();
                }
                this.jwt = result.Jwt;
                WebSocket ws = new WebSocket(result.EventStreamUri);
 


                ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                ws.OnOpen += (sender, e) => {
                    Console.WriteLine("Connection Opened");
                };

                ws.OnMessage += (sender, e) =>
                {
                    Dictionary<string, object> notification = JsonConvert.DeserializeObject<Dictionary<string, object>>(e.Data);
                    object metadata;
                    if (notification.TryGetValue("metadata", out metadata))
                    {
                        Dictionary<string, object> mdObject = ((JObject)metadata).ToObject<Dictionary<string, object>>();
                        object type;
                        if (mdObject.TryGetValue("type", out type))
                        {
                            switch (type.ToString())
                            {
                                case "message":
                                    ProcessMessage(notification);
                                    break;
                                case "member-join":
                                    break;
                                default:
                                    break;
                            }
                        }
                    }




                };

                ws.OnError += (sender, e) => {
                    Console.WriteLine("Error: " + e.Message);
                };

                ws.OnClose += (sender, e) => {
                    Console.WriteLine("Closed ");
                };
                ws.Connect();

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception when calling WebChatApi.PostWebchatGuestConversations: " + e.Message);
            }

        }
    }
}
