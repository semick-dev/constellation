import { BlobServiceClient } from "@azure/storage-blob";
import { QueueClient, QueueSendMessageResponse, QueueServiceClient } from "@azure/storage-queue";

export class WatcherPayload {
    PayloadType: string;
    QualitySelection: string;
    Url: string;

    constructor(payloadType: string, qualitySelection: string, url: string){
        this.PayloadType = payloadType;
        this.QualitySelection = qualitySelection;
        this.Url = url;
    }
}

function jsonToBase64(jsonObj: any) {
    const jsonString = JSON.stringify(jsonObj)
    return  Buffer.from(jsonString).toString('base64')
}
function encodeBase64ToJson(base64String: string) {
    const jsonString = Buffer.from(base64String,'base64').toString()
    return JSON.parse(jsonString)
}

export class WatcherClient {
    Blob: BlobServiceClient;
    Queue: QueueClient;

    constructor(connectionString: string, queueName: string){
        this.Blob = BlobServiceClient.fromConnectionString(connectionString);
        console.log(this.Blob);

        this.Queue = QueueServiceClient.fromConnectionString(connectionString).getQueueClient(queueName);
        console.log(this.Queue);
    }

    async enqueue(payload: WatcherPayload): Promise<QueueSendMessageResponse> {
        let message: string = jsonToBase64(payload);

        console.log(message);

        let result = await this.Queue.sendMessage(message);
        return result;
    }

    async dequeue(): Promise<WatcherPayload> {
        let receivedMessages = await this.Queue.receiveMessages();
        var message = receivedMessages.receivedMessageItems[0];
        await this.Queue.deleteMessage(message.messageId, message.popReceipt);

        let obj = encodeBase64ToJson(message.messageText) as WatcherPayload;
        return obj;
    }
}