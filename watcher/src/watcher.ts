import { BlobServiceClient } from "@azure/storage-blob";
import { QueueClient, QueueSendMessageResponse, QueueServiceClient } from "@azure/storage-queue";

export class WatcherPayload {
    PayloadType: string;
    QualitySelection: string;
    Url: string;
    VideoId: string;

    constructor(payloadType: string, qualitySelection: string, url: string){
        this.PayloadType = payloadType;
        this.QualitySelection = qualitySelection;
        this.Url = url;
        this.VideoId = extractVideoId(this);
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

export var lastSet: WatcherPayload[];

function getCurrentAssignations(containingElement: HTMLElement): Array<string> {
    let currentAssignationsTable = document.getElementById('sidepaneldata');

    // we've already populated, we gotta scrape
    if (currentAssignationsTable != null){
        let existingAssignations: HTMLCollectionOf<Element> = document.getElementsByClassName('previewItem')
        let idsInOrder: Array<string> = [];

        for (let i = 0; i < existingAssignations.length; i++) {
            idsInOrder.push(existingAssignations[i].id);
        }
        
        return idsInOrder;
    }

    return [];
}

function generateTable(): string {
    // initialize table, then get existing assignations to fill the ready ids
    let completeTable: string = `
    <h2>Queued Items</h2>
    <table id="sidepaneldata" style="width:100%; padding: 0; margin: 0" cellspacing="0" cellpadding="0">
        <tr>
            <td class="queueItem">
                <iframe
                    id="id0"
                    class="previewItem"
                    src=""
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
                </iframe>
            </td>
            <td class="queueItem">
                <iframe
                    id="id1"
                    class="previewItem"
                    src=""
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
                </iframe>
            </td>
            <td class="queueItem">
                <iframe
                    id="id2"
                    class="previewItem"
                    src=""
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
                </iframe>
            </td>
            <td class="queueItem">
                <iframe
                    id="id3"
                    class="previewItem"
                    src=""
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
                </iframe>
            </td>
        </tr>

        <tr>
            <td class="queueItem">
                <iframe
                    id="id4"
                    class="previewItem"
                    src=""
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
                </iframe>
            </td>
            <td class="queueItem">
                <iframe
                    id="id5"
                    class="previewItem"
                    src=""
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
                </iframe>
            </td>
            <td class="queueItem">
                <iframe
                    id="id6"
                    class="previewItem"
                    src=""
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
                </iframe>
            </td>
            <td class="queueItem">
                <iframe
                    id="id7"
                    class="previewItem"
                    src=""
                    frameborder="0"
                    allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen>
                </iframe>
            </td>
        </tr>
    </table>
    `
    return completeTable;
}

function updateCell(existingId: string, update: WatcherPayload): void {
    let element = document.getElementById(existingId) as HTMLIFrameElement;

    if (element != null){
        if (update.VideoId.length > 0) {
            element.src = `https://www.youtube.com/embed/${update.VideoId}`
            element.id = update.VideoId;
        }
        else {
            // only update if it's not already blank
            if (!element.id.startsWith("fake")){
                element.src = ``;
                element.id = "fake" + crypto.randomUUID();
            }
        }
    }
    else {
        console.log(`Could not update cell of target id ${existingId}. Don't know what to do with ${update.Url}`)
    }
}

function isAssigned(id: string): boolean {
    return  document.getElementById(id) != null;
}

function removePayload(id: string|WatcherPayload, items: WatcherPayload[]): void {
    if (id instanceof Object){
        const index =  items.findIndex(x => x.VideoId == id.VideoId);
        if (index > -1) {
            items.splice(index, 1);
        }
    }
    // string
    else {
        const index =  items.findIndex(x => x.VideoId == id);
        if (index > -1) {
            items.splice(index, 1);
        } 
    }
}

export function SetQueuedItems(containingElement: HTMLElement, payloads: Array<WatcherPayload>): void {
    let waitingForAssignation: string[] = [];
    let itemsForAssignation: WatcherPayload[] = payloads.slice(0, 8);
    let existingAssignations: string[] = getCurrentAssignations(containingElement); // 8 long too

    if (existingAssignations.length === 0) {
        containingElement.innerHTML = generateTable();
        existingAssignations = getCurrentAssignations(containingElement);
    }
    
    // walk across the existing assignations
    for (var i = 0; i < existingAssignations.length; i++) {
        let targetId = existingAssignations[i];
        let indexInItems = itemsForAssignation.findIndex(x => x.VideoId == targetId);

        // if this target id (which already exists on the page) exists in items for assignation, our work is complete for now
        if (indexInItems > -1){
            removePayload(itemsForAssignation[indexInItems], itemsForAssignation);
        }
        else {
            // these cells dont have a place in the queue, and so should be cleared
            waitingForAssignation.push(targetId);
        }
    }

    // because plain string arrays can only be appended, we need to reverse to get into the same order as the display order again
    waitingForAssignation = waitingForAssignation.reverse()

    // now walk across the items we have for assignation, there should be succicient cells available
    // to override
    for (var i = 0; i < itemsForAssignation.length; i++) {
        let payload = itemsForAssignation[i];
        let idForReplacement = waitingForAssignation.pop();

        if (idForReplacement !== undefined){
            updateCell(idForReplacement, payload);
        }
        else {
            console.log("No cell available to update, waiting to assign" + payload.VideoId );
        }
    }
    
    // if there are remaining cells, that means that we have less queue items than remaining cells to fill with an ifram
    for (var i = 0; i < waitingForAssignation.length; i++) {
        updateCell(waitingForAssignation[i], { VideoId: "", Url: "" } as WatcherPayload)
    }
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
    }, 1000);

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

        return messages.peekedMessageItems.map(element => {
            return encodeBase64ToJson(element.messageText) as WatcherPayload;
        });
    }
}