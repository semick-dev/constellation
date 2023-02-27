from azure.storage.queue import QueueMessage
import base64, json

class WatcherPayload:
    def __init__(self, payload_type: str, quality_selection: str, url: str):
        self.PayloadType = payload_type
        self.QualitySelection = quality_selection
        self.Url = url

    @classmethod
    def from_message(cls, msg: QueueMessage):
        jcontent = json.loads(base64.b64decode(msg.content))
        return WatcherPayload(**jcontent)
