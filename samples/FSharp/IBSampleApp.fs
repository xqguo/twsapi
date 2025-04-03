open System
open System.Threading
open IBApi

let getFieldName fieldId =
    match fieldId with
    | 0 -> "Bid Size"
    | 1 -> "Bid Price"
    | 2 -> "Ask Price"
    | 3 -> "Ask Size"
    | 4 -> "Last Price"
    | 5 -> "Last Size"
    | 6 -> "High Price"
    | 7 -> "Low Price"
    | 8 -> "Volume"
    | 9 -> "Close Price"
    | _ -> sprintf "Unknown Field (%d)" fieldId

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
            printfn "Account Summary: ReqId=%d, Account=%s, Tag=%s, Value=%s, Currency=%s" reqId account tag value currency
            match tag with
            | "NetLiquidation" ->
                printfn "Net Liquidation Value: ReqId=%d, Account=%s, Value=%s %s" reqId account value currency
                accountSummaryEvent.Set() |> ignore
            | "TotalCashValue" ->
                printfn "Total Cash Balance: ReqId=%d, Account=%s, Currency=%s, Balance=%s" reqId account currency value
                accountSummaryEvent.Set() |> ignore
            | _ -> ()

        member _.accountSummaryEnd(reqId: int) =
            printfn "Account Summary Request Completed: ReqId=%d" reqId

        // Market data-related methods
        member _.tickPrice(tickerId: int, field: int, price: float, attribs: TickAttrib) =
            let fieldName = getFieldName field
            printfn "Tick Price: TickerId=%d, Field=%s, Price=%.2f" tickerId fieldName price

        member _.tickSize(tickerId: int, field: int, size: Decimal) =
            let fieldName = getFieldName field
            printfn "Tick Size: TickerId=%d, Field=%s, Size=%M" tickerId fieldName size

        member _.tickString(tickerId: int, field: int, value: string) =
            let fieldName = getFieldName field
            printfn "Tick String: TickerId=%d, Field=%s, Value=%s" tickerId fieldName value

        member _.tickGeneric(tickerId: int, field: int, value: float) =
            let fieldName = getFieldName field
            printfn "Tick Generic: TickerId=%d, Field=%s, Value=%.2f" tickerId fieldName value

        member _.tickSnapshotEnd(tickerId: int) =
            printfn "Tick Snapshot End: TickerId=%d" tickerId

        member _.tickEFP(tickerId: int, tickType: int, basisPoints: float, formattedBasisPoints: string, impliedFuture: float, holdDays: int, futureLastTradeDate: string, dividendImpact: float, dividendsToLastTradeDate: float) =
            printfn "tickEFP: TickerId=%d, TickType=%d, BasisPoints=%.2f, FormattedBasisPoints=%s, ImpliedFuture=%.2f, HoldDays=%d, FutureLastTradeDate=%s, DividendImpact=%.2f, DividendsToLastTradeDate=%.2f"
                tickerId tickType basisPoints formattedBasisPoints impliedFuture holdDays futureLastTradeDate dividendImpact dividendsToLastTradeDate

        member _.tickOptionComputation(tickerId: int, field: int, tickAttrib: int, impliedVolatility: float, delta: float, optPrice: float, pvDividend: float, gamma: float, vega: float, theta: float, undPrice: float) =
            printfn "tickOptionComputation: TickerId=%d, Field=%d, TickAttrib=%d, ImpliedVolatility=%.2f, Delta=%.2f, OptPrice=%.2f, PvDividend=%.2f, Gamma=%.2f, Vega=%.2f, Theta=%.2f, UndPrice=%.2f"
                tickerId field tickAttrib impliedVolatility delta optPrice pvDividend gamma vega theta undPrice

        // Error handling
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
        clientSocket.reqAccountSummary(reqId, "All", "TotalCashValue")

        // Wait for the response or timeout after 10 seconds
        if not (accountSummaryEvent.WaitOne(10000)) then
            printfn "Timeout waiting for Cash Balance response."

    member this.RetrieveAllPositions() =
        printfn "Requesting all positions..."
        clientSocket.reqPositions()

    member this.RequestMarketData() =
        printfn "Requesting market data for GOOG, 700 HK, and GCZ05..."

        // Define contracts for the symbols
        let googContract = new Contract()
        googContract.Symbol <- "GOOG"
        googContract.SecType <- "STK"
        googContract.Exchange <- "SMART"
        googContract.Currency <- "USD"

        let hk700Contract = new Contract()
        hk700Contract.Symbol <- "700"
        hk700Contract.SecType <- "STK"
        hk700Contract.Exchange <- "SEHK"
        hk700Contract.Currency <- "HKD"

        let gcz05Contract = new Contract()
        gcz05Contract.Symbol <- "GCZ05"
        gcz05Contract.SecType <- "FUT"
        gcz05Contract.Exchange <- "NYMEX"
        gcz05Contract.Currency <- "USD"

        // Request market data
        clientSocket.reqMktData(1, googContract, "", true, false, null) // Real-time data for GOOG
        clientSocket.reqMktData(2, hk700Contract, "", true, false, null) // Real-time data for 700 HK
        clientSocket.reqMktData(3, gcz05Contract, "", true, false, null) // Real-time data for GCZ05

[<EntryPoint>]
let main argv =
    let client = IBClient()
    client.Connect()

    // View Net Liquidation Value
    client.ViewNetLiquidationValue()

    // View Net Liquidation Value
    client.ViewCashBalances()
    client.RetrieveAllPositions()
    client.RequestMarketData()

    Thread.Sleep(5000)
    client.Disconnect() |> ignore
    0

