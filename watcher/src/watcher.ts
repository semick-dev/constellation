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

function extractVideoId(payload: WatcherPayload): string {
    switch(payload.PayloadType){
        case "youtubedl":
            var uri = new URL(payload.Url);

            // https://youtu.be/tNw8y3EQAtI?list=FL7Nyaz8DK7bhCUA3ZcvvrmA;
            // https://youtu.be/y8BUc_aYZ-M;
            if (payload.Url.startsWith("https://youtu.be"))
            {
                return uri.pathname.slice(1);
            }

            // https://www.youtube.com/watch?v=y8BUc_aYZ-M
            // https://www.youtube.com/watch?v=9PLPfL6Tge4&list=PLYFEK0EdxB0oSLDSttKm6aAI-lxI4G3FD
            else if (payload.Url.startsWith("https://www.youtube.com"))
            {
                var queries = uri.search.split("&");
                for (let i = 0; i < queries.length; i++)
                {
                    let query = queries[i];
                    if (query.indexOf("v=") != -1){
                        return query.split("=")[1];
                    }
                }
            }

            return "Unknown Video Id";
        default:
            return payload.Url;
    }
}

export function SetOutputWindowStatus(classStatus: string, message: string): void {
    let classlist: string[] = ["success", "failure", "loading", "unknown"];
    let outputWindow: HTMLElement | null = document.getElementById("viewport");

    if (outputWindow != null){
        for(var i = 0; i < classlist.length; i++){
            outputWindow.classList.remove(classlist[i]);
        }
        outputWindow.classList.add(classStatus);
        outputWindow.innerHTML = message;
    }
}

export function SetQueuedItems(containingElement: HTMLElement, payloads: Array<WatcherPayload>): void {
    // todo: ensure that if the set of messages is the same, we don't refresh.
    console.log(payloads);

    let completeTable: string = `<h3>Queued Items</h3><table id="sidepaneldata">`
    payloads.forEach(payload => {
        let videoId: string = extractVideoId(payload)
        let tableTier: string = `<tr><td>
        <iframe
            class="previewItem"
            src="https://www.youtube.com/embed/${videoId}"
            frameborder="0"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
        </iframe></td></tr>`

        completeTable += tableTier
    });
    completeTable += "</table>"

    containingElement.innerHTML = completeTable;
}

export function StartQueueWatcher(containingElement: HTMLElement, connectionElement: string):  NodeJS.Timeout {
    let interval = setInterval( async () => {
        try {
            let cs = (document.getElementById(connectionElement) as any).value
            let client = new WatcherClient(cs, "watcher");
            let payloads: WatcherPayload[] = await client.peek_messages()
            SetQueuedItems(containingElement, payloads);
        }
        catch(err) {
        }
    }, 10000);

    return interval;
}

export function StopQueueWatcher(timeout: NodeJS.Timeout): void {
    clearInterval(timeout);
}


// We are deliberately not catching exceptions in all functions herein. This aids in easier UI response try/catch usage
export class WatcherClient {
    Blob: BlobServiceClient;
    Queue: QueueClient;

    constructor(connectionString: string, queueName: string){
        this.Blob = BlobServiceClient.fromConnectionString(connectionString);
        this.Queue = QueueServiceClient.fromConnectionString(connectionString).getQueueClient(queueName);
    }

    async enqueue(payload: WatcherPayload): Promise<QueueSendMessageResponse> {
        let message: string = jsonToBase64(payload);
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

    async peek_messages(): Promise<Array<WatcherPayload>> {
        let messages = await this.Queue.peekMessages({ numberOfMessages: 10 });

        console.log(messages);

        return messages.peekedMessageItems.map(element => {
            return encodeBase64ToJson(element.messageText) as WatcherPayload;
        });
    }
}