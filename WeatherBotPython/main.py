import http.server
import requests
import json
import asyncio
from botbuilder.schema import (Activity, ActivityTypes,
                               AnimationCard, AudioCard, Attachment,
                               ActionTypes, CardAction,
                               CardImage, HeroCard,
                               MediaUrl, ThumbnailUrl,
                               ThumbnailCard, VideoCard,
                               ReceiptCard, SigninCard,
                               Fact, ReceiptItem)
from botframework.connector import ConnectorClient
from botframework.connector.auth import (MicrosoftAppCredentials,
                                         JwtTokenValidation, SimpleCredentialProvider)
from botbuilder.core import (BotFrameworkAdapter, BotFrameworkAdapterSettings, TurnContext,
                            ConversationState, MemoryStorage, UserState, CardFactory)
from datetime import datetime
from adaptive_card_front import ADAPTIVE_CARD_CONTENT

APP_ID = ''
APP_PASSWORD = ''

ACCUWEATHER = 'http://dataservice.accuweather.com'
API_KEY = '?apikey=gkK3AZCNv9AUq43XGUA4WYYXvTrDUBGV'
LOCATION = ACCUWEATHER + '/locations/v1/cities/autocomplete' + API_KEY + "&q="
FORECAST = ACCUWEATHER + '/forecasts/v1/daily/1day/'


class BotRequestHandler(http.server.BaseHTTPRequestHandler):

    @staticmethod
    def __create_reply_activity(request_activity, text):
        return Activity(
            type=ActivityTypes.message,
            channel_id=request_activity.channel_id,
            conversation=request_activity.conversation,
            recipient=request_activity.from_property,
            from_property=request_activity.recipient,
            service_url=request_activity.service_url,
            text=text
            )

    def __create_reply_card_activity(self, request_activity, card):
        return Activity(
            type=ActivityTypes.message,
            channel_id=request_activity.channel_id,
            conversation=request_activity.conversation,
            recipient=request_activity.from_property,
            from_property=request_activity.recipient,
            service_url=request_activity.service_url,
            attachments=[card]
            )

    def __handle_conversation_update_activity(self, activity):
        self.send_response(202)
        self.end_headers()
        if activity.members_added[0].id != activity.recipient.id:
            credentials = MicrosoftAppCredentials(APP_ID, APP_PASSWORD)
            reply = BotRequestHandler.__create_reply_activity(activity, 'Привет, пользователь!')
            connector = ConnectorClient(credentials, base_url=reply.service_url)
            connector.conversations.send_to_conversation(reply.conversation.id, reply)

    def __handle_message_activity(self, activity):
        self.send_response(200)
        self.end_headers()
        credentials = MicrosoftAppCredentials(APP_ID, APP_PASSWORD)
        connector = ConnectorClient(credentials, base_url=activity.service_url)
        reply = BotRequestHandler.__create_reply_card_activity(self, activity, BotRequestHandler.__answer(self, activity.text))
        connector.conversations.send_to_conversation(reply.conversation.id, reply)

    def __handle_authentication(self, activity):
        credential_provider = SimpleCredentialProvider(APP_ID, APP_PASSWORD)
        loop = asyncio.new_event_loop()
        try:
            loop.run_until_complete(JwtTokenValidation.authenticate_request(
                activity, self.headers.get("Authorization"), credential_provider))
            return True
        except Exception as ex:
            self.send_response(401, ex)
            self.end_headers()
            return False
        finally:
            loop.close()

    def __unhandled_activity(self):
        self.send_response(404)
        self.end_headers()

    def do_POST(self):
        body = self.rfile.read(int(self.headers['Content-Length']))
        data = json.loads(str(body, 'utf-8'))
        activity = Activity.deserialize(data)

        if not self.__handle_authentication(activity):
            return

        if activity.type == ActivityTypes.conversation_update.value:
            self.__handle_conversation_update_activity(activity)
        elif activity.type == ActivityTypes.message.value:
            self.__handle_message_activity(activity)
        else:
            self.__unhandled_activity()

    def __answer(self, text):
        location_response = requests.get(LOCATION + text + '&language=ru-ru')
        city = location_response.json()[0]
        forecast_response = requests.get(FORECAST + city["Key"] + API_KEY + '&language=ru-ru&details=true&metric=true')
        replycard = BotRequestHandler.__create_forecast_card(self, city, forecast_response.json())
        return replycard

    def __create_forecast_card(self, city, forecast):
        adapt_dict = ADAPTIVE_CARD_CONTENT
        adapt_dict["body"][0]["text"] = city["LocalizedName"]
        adapt_dict["body"][1]["text"] = str(forecast["Headline"]["EffectiveDate"])
        adapt_dict["body"][2]["columns"][1]["items"][0]["text"] = str(forecast["DailyForecasts"][0]["Temperature"]["Minimum"]["Value"])
        adapt_dict["body"][2]["columns"][2]["items"][1]["text"] = forecast["Headline"]["Text"]
        adapt_dict["body"][2]["columns"][2]["items"][0]["text"] = "RealFeel: " + str(forecast["DailyForecasts"][0]["RealFeelTemperature"]["Minimum"]["Value"])
        return CardFactory.adaptive_card(adapt_dict)

    
        

try:
    SERVER = http.server.HTTPServer(('localhost', 9000), BotRequestHandler)
    print('Started http server on localhost:9000')
    SERVER.serve_forever()
except KeyboardInterrupt:
    print('^C received, shutting down server')
    SERVER.socket.close()