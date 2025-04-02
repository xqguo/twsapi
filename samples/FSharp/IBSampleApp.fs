open System
open System.Threading
open IBApi

type IBClient() as this =
    let mutable nextOrderId = 0
    let signal = new EReaderMonitorSignal()
    let clientSocket = new EClientSocket(this :> EWrapper, signal)

    // Initialize reader
    let mutable reader = Unchecked.defaultof<EReader>

    // Synchronization for account summary
    let accountSummaryEvent = new ManualResetEvent(false)

    interface EWrapper with
        // Account summary-related methods
        member _.accountSummary(reqId: int, account: string, tag: string, value: string, currency: string) =
            if tag = "NetLiquidation" then
                printfn "Net Liquidation Value: ReqId=%d, Account=%s, Value=%s %s" reqId account value currency
                accountSummaryEvent.Set() // Signal that the response has been received

        member _.accountSummaryEnd(reqId: int) =
            printfn "Account Summary Request Completed: ReqId=%d" reqId

        // Error handlers
        member _.error(id: int, errorCode: int, errorMsg: string, advancedOrderRejectJson: string) =
            printfn "Error: Id=%d, Code=%d, Msg=%s" id errorCode errorMsg

        member _.error(e: exn) = printfn "Error: %s" e.Message
        member _.error(str: string) = printfn "Error: %s" str

        // Connection-related methods
        member _.connectionClosed() =
            printfn "Connection closed"

        member _.nextValidId(orderId: int) =
            nextOrderId <- orderId
            printfn "Next Valid Id: %d" orderId

        // Portfolio and account-related methods
        member _.updatePortfolio(contract: Contract, position: decimal, marketPrice: float, marketValue: float, averageCost: float, unrealizedPNL: float, realizedPNL: float, accountName: string) = ()
        member _.updateAccountTime(timestamp: string) = ()
        member _.accountDownloadEnd(account: string) = ()

        // Order-related methods
        member _.orderStatus(orderId: int, status: string, filled: decimal, remaining: decimal, avgFillPrice: float, permId: int, parentId: int, lastFillPrice: float, clientId: int, whyHeld: string, mktCapPrice: float) = ()
        member _.openOrder(orderId: int, contract: Contract, order: Order, orderState: OrderState) = ()
        member _.openOrderEnd() = ()

        // Contract-related methods
        member _.contractDetails(reqId: int, contractDetails: ContractDetails) = ()
        member _.contractDetailsEnd(reqId: int) = ()
        member _.bondContractDetails(reqId: int, contract: ContractDetails) = ()

        // Execution-related methods
        member _.execDetails(reqId: int, contract: Contract, execution: Execution) = ()
        member _.execDetailsEnd(reqId: int) = ()

        // Historical data-related methods
        member _.historicalData(reqId: int, bar: Bar) = ()
        member _.historicalDataUpdate(reqId: int, bar: Bar) = ()
        member _.historicalDataEnd(reqId: int, start: string, ``end``: string) = ()

        // Market data-related methods
        member _.marketDataType(reqId: int, marketDataType: int) = ()
        member _.updateMktDepth(tickerId: int, position: int, operation: int, side: int, price: float, size: decimal) = ()
        member _.updateMktDepthL2(tickerId: int, position: int, marketMaker: string, operation: int, side: int, price: float, size: decimal, isSmartDepth: bool) = ()

        // News-related methods
        member _.updateNewsBulletin(msgId: int, msgType: int, message: string, origExchange: string) = ()

        // Position-related methods
        member _.position(account: string, contract: Contract, pos: decimal, avgCost: float) = ()
        member _.positionEnd() = ()

        // Real-time bar-related methods
        member _.realtimeBar(reqId: int, date: int64, ``open``: float, high: float, low: float, close: float, volume: decimal, WAP: decimal, count: int) = ()

        // Financial advisor-related methods
        member _.receiveFA(faDataType: int, faXmlData: string) = ()

        // Tick request parameters
        member _.tickReqParams(tickerId: int, minTick: float, bboExchange: string, snapshotPermissions: int) = ()

        // Historical ticks
        member _.historicalTicks(reqId: int, ticks: HistoricalTick array, ``done``: bool) = ()
        member _.historicalTicksBidAsk(reqId: int, ticks: HistoricalTickBidAsk array, ``done``: bool) = ()
        member _.historicalTicksLast(reqId: int, ticks: HistoricalTickLast array, ``done``: bool) = ()

        // Tick-by-tick data
        member _.tickByTickAllLast(reqId: int, tickType: int, time: int64, price: float, size: decimal, tickAttribLast: TickAttribLast, exchange: string, specialConditions: string) = ()
        member _.tickByTickBidAsk(reqId: int, time: int64, bidPrice: float, askPrice: float, bidSize: decimal, askSize: decimal, tickAttribBidAsk: TickAttribBidAsk) = ()
        member _.tickByTickMidPoint(reqId: int, time: int64, midPoint: float) = ()

        // Commission report
        member _.commissionReport(commissionReport: CommissionReport) = ()

        // Order-bound methods
        member _.orderBound(orderId: int64, apiClientId: int, apiOrderId: int) = ()

        // Completed orders
        member _.completedOrder(contract: Contract, order: Order, orderState: OrderState) = ()
        member _.completedOrdersEnd() = ()

        // Other required methods (stub implementations)
        member _.tickPrice(_, _, _, _) = ()
        member _.tickSize(_, _, _) = ()
        member _.tickString(_, _, _) = ()
        member _.tickGeneric(_, _, _) = ()
        member _.tickEFP(_, _, _, _, _, _, _, _, _) = ()
        member _.tickOptionComputation(_, _, _, _, _, _, _, _, _, _, _) = ()
        member _.tickSnapshotEnd(_) = ()
        member _.managedAccounts(_) = ()
        member _.updateAccountValue(_, _, _, _) = ()
        member _.currentTime(_) = ()
        member _.deltaNeutralValidation(_, _) = ()
        member _.fundamentalData(_, _) = ()
        member _.marketRule(_, _) = ()
        member _.pnl(_, _, _, _) = ()
        member _.pnlSingle(_, _, _, _, _, _) = ()
        member _.scannerData(_, _, _, _, _, _, _) = ()
        member _.scannerDataEnd(_) = ()
        member _.scannerParameters(_) = ()
        member _.securityDefinitionOptionParameter(_, _, _, _, _, _, _) = ()
        member _.securityDefinitionOptionParameterEnd(_) = ()
        member _.smartComponents(_, _) = ()
        member _.softDollarTiers(_, _) = ()
        member _.newsProviders(_) = ()
        member _.familyCodes(_) = ()
        member _.symbolSamples(_, _) = ()
        member _.mktDepthExchanges(_) = ()
        member _.tickNews(_, _, _, _, _, _) = ()
        member _.newsArticle(_, _, _) = ()
        member _.historicalNews(_, _, _, _, _) = ()
        member _.historicalNewsEnd(_, _) = ()
        member _.headTimestamp(_, _) = ()
        member _.histogramData(_, _) = ()
        member _.rerouteMktDataReq(_, _, _) = ()
        member _.rerouteMktDepthReq(_, _, _) = ()
        member _.verifyMessageAPI(_) = ()
        member _.verifyCompleted(_, _) = ()
        member _.verifyAndAuthMessageAPI(_, _) = ()
        member _.verifyAndAuthCompleted(_, _) = ()
        member _.connectAck() = ()
        member _.displayGroupList(_, _) = ()
        member _.displayGroupUpdated(_, _) = ()
        member _.positionMulti(_, _, _, _, _, _) = ()
        member _.positionMultiEnd(_) = ()
        member _.accountUpdateMulti(_, _, _, _, _, _) = ()
        member _.accountUpdateMultiEnd(_) = ()
        member _.replaceFAEnd(_, _) = ()
        member _.wshMetaData(_, _) = ()
        member _.wshEventData(_, _) = ()
        member _.historicalSchedule(_, _, _, _, _) = ()
        member _.userInfo(_, _) = ()

    member this.Connect() =
        clientSocket.eConnect("127.0.0.1", 7496, 0)
        printfn "Connected to TWS"

        // Start EReader to process messages
        reader <- new EReader(clientSocket, signal)
        reader.Start()
        let readerThread = new Thread(ThreadStart(fun () ->
            while clientSocket.IsConnected() do
                signal.waitForSignal()
                reader.processMsgs()
        ))
        readerThread.IsBackground <- true
        readerThread.Start()

    member this.Disconnect() =
        clientSocket.eDisconnect()
        printfn "Disconnected from TWS"

    member this.ViewNetLiquidationValue(account: string) =
        let reqId = nextOrderId
        nextOrderId <- nextOrderId + 1
        accountSummaryEvent.Reset() // Reset the event before making the request
        clientSocket.reqAccountSummary(reqId, "All", "NetLiquidation")
        printfn "Requested Net Liquidation Value for Account=%s with ReqId=%d" account reqId

        // Wait for the response or timeout after 10 seconds
        if not (accountSummaryEvent.WaitOne(10000)) then
            printfn "Timeout waiting for Net Liquidation Value response."

[<EntryPoint>]
let main argv =
    let client = IBClient()
    client.Connect()

    // View Net Liquidation Value
    client.ViewNetLiquidationValue("U8865335")  // Replace with your account ID

    client.Disconnect()
    0
