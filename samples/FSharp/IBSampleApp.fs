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
            match tag with
            | "NetLiquidation" ->
                printfn "Net Liquidation Value: ReqId=%d, Account=%s, Value=%s %s" reqId account value currency
            | "CashBalance" ->
                printfn "Cash Balance: ReqId=%d, Account=%s, Currency=%s, Balance=%s" reqId account currency value
                accountSummaryEvent.Set() |> ignore // Signal that the response has been received
            | _ -> ()

        member _.accountSummaryEnd(reqId: int) =
            printfn "Account Summary Request Completed: ReqId=%d" reqId

        // Error handlers
        /// <summary></summary>
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

        member _.position(account: string, contract: Contract, pos: decimal, avgCost: float) =
            printfn "Position: Account=%s, Contract=%s, Position=%M, AvgCost=%.2f" account contract.Symbol pos avgCost

        member _.positionEnd() =
            printfn "All positions retrieved."

// Other required methods (stub implementations)
        member _.updatePortfolio(_, _, _, _, _, _, _, _) = ()
        member _.updateAccountTime(_) = ()
        member _.accountDownloadEnd(_) = ()
        member _.orderStatus(_, _, _, _, _, _, _, _, _, _, _) = ()
        member _.openOrder(_, _, _, _) = ()
        member _.openOrderEnd() = ()
        member _.contractDetails(_, _) = ()
        member _.contractDetailsEnd(_) = ()
        member _.bondContractDetails(_, _) = ()
        member _.execDetails(_, _, _) = ()
        member _.execDetailsEnd(_) = ()
        member _.historicalData(_, _) = ()
        member _.historicalDataUpdate(_, _) = ()
        member _.historicalDataEnd(_, _, _) = ()
        member _.marketDataType(_, _) = ()
        member _.updateMktDepth(_, _, _, _, _, _) = ()
        member _.updateMktDepthL2(_, _, _, _, _, _, _, _) = ()
        member _.updateNewsBulletin(_, _, _, _) = ()
        member _.realtimeBar(_, _, _, _, _, _, _, _, _) = ()
        member _.receiveFA(_, _) = ()
        member _.tickReqParams(_, _, _, _) = ()
        member _.historicalTicks(_, _, _) = ()
        member _.historicalTicksBidAsk(_, _, _) = ()
        member _.historicalTicksLast(_, _, _) = ()
        member _.tickByTickAllLast(_, _, _, _, _, _, _, _) = ()
        member _.tickByTickBidAsk(_, _, _, _, _, _, _) = ()
        member _.tickByTickMidPoint(_, _, _) = ()
        member _.commissionReport(_) = ()
        member _.orderBound(_, _, _) = ()
        member _.completedOrder(_, _, _) = ()
        member _.completedOrdersEnd() = ()
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

    member this.ViewNetLiquidationValue() =
        printfn "Requesting Net Liquidation Value..."
        let reqId = nextOrderId
        nextOrderId <- nextOrderId + 1
        accountSummaryEvent.Reset() |> ignore // Reset the event before making the request
        clientSocket.reqAccountSummary(reqId, "All", "NetLiquidation")

        // Wait for the response or timeout after 10 seconds
        if not (accountSummaryEvent.WaitOne(10000)) then
            printfn "Timeout waiting for Net Liquidation Value response."

    member this.ViewCashBalances() =
        printfn "Requesting Cash Balances..."
        let reqId = nextOrderId
        nextOrderId <- nextOrderId + 1
        accountSummaryEvent.Reset() |> ignore // Reset the event before making the request
        clientSocket.reqAccountSummary(reqId, "All", "CashBalance")

        // Wait for the response or timeout after 10 seconds
        if not (accountSummaryEvent.WaitOne(10000)) then
            printfn "Timeout waiting for Cash Balance response."

    member this.RetrieveAllPositions() =
        printfn "Requesting all positions..."
        clientSocket.reqPositions()

[<EntryPoint>]
let main argv =
    let client = IBClient()
    client.Connect()

    // View Net Liquidation Value
    client.ViewNetLiquidationValue()

    // View Net Liquidation Value
    client.ViewCashBalances()
    client.RetrieveAllPositions()

    Thread.Sleep(5000)
    client.Disconnect() |> ignore
    0

