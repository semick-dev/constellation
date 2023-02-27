// copied from watcher/watcher.ts and here for reference only
class ExampleWatcherPayload {
    PayloadType: string;
    QualitySelection: string;
    Url: string;

    constructor(payloadType: string, qualitySelection: string, url: string){
        this.PayloadType = payloadType;
        this.QualitySelection = qualitySelection;
        this.Url = url;
    }
}