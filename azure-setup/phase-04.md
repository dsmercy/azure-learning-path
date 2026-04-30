# Phase 4 — Azure Setup Guide
## Service Bus + Event Grid + Event Hubs

---

## Cost Warning — Read First

| Service | Tier | Cost |
|---------|------|------|
| Service Bus | **Basic** (queues only) | ~$0.05/month at learning scale |
| Service Bus | **Standard** (+ topics) | ~$10/month |
| Event Grid | First 100K ops | **FREE** |
| Event Hubs | Basic, first 10M events | **FREE (12-month)** |

> ✅ Use **Basic tier Service Bus** for learning — queues only, no topics.
> Topics require Standard ($10/mo). The demos are written to work with Basic.
> If you want to learn topics, upgrade to Standard for just a few days then downgrade.

---

## What You Are Creating

```
Resource Group: rg-learn-phase4
├── Service Bus Namespace: sbns-learning-XXXX  (Basic — queues only)
│   └── Queue: orders-queue
├── Event Grid Topic: egt-learning-events       (custom topic)
└── Event Hubs Namespace: ehns-learning-XXXX    (Basic)
    └── Event Hub: telemetry-hub
```

---

## Step 1 — Create Resource Group

```bash
az group create --name rg-learn-phase4 --location eastus
```

---

## Step 2 — Create Service Bus Namespace

```bash
az servicebus namespace create \
  --name sbns-learning-YOURNAME \
  --resource-group rg-learn-phase4 \
  --location eastus \
  --sku Basic
```

**Concepts:**
- **Namespace** = the container (like a server for queues/topics)
- **Queue** = FIFO message store. One sender, one consumer.
- **Topic** (Standard+ only) = publish once, multiple subscribers receive

---

## Step 3 — Create a Queue

```bash
az servicebus queue create \
  --name orders-queue \
  --namespace-name sbns-learning-YOURNAME \
  --resource-group rg-learn-phase4 \
  --max-delivery-count 5
```

`--max-delivery-count 5` means: if processing fails 5 times, the message goes to the **dead-letter queue** (a holding area for failed messages you can inspect).

---

## Step 4 — Get Service Bus Connection String

```bash
az servicebus namespace authorization-rule keys list \
  --resource-group rg-learn-phase4 \
  --namespace-name sbns-learning-YOURNAME \
  --name RootManageSharedAccessKey \
  --query "primaryConnectionString" -o tsv
```

**Copy to `context/project-context.md`.**

---

## Step 5 — Explore Service Bus in Azure Portal

1. portal.azure.com → Service Bus → your namespace
2. Click **Queues** → click `orders-queue`
3. Click **Service Bus Explorer** → you can send and receive messages directly in the portal!
   - Try sending a message: `{"orderId": 1, "product": "test"}`
   - Then receive it — it disappears from the queue
4. Check the **Metrics** tab — Incoming/Outgoing Messages chart

---

## Step 6 — Create Event Grid Topic

```bash
az eventgrid topic create \
  --name egt-learning-events \
  --resource-group rg-learn-phase4 \
  --location eastus
```

**Concepts:**
- **Topic** = where you publish events TO
- **Subscription** = who receives events FROM the topic (webhook, queue, function)
- Event Grid is push-based: it calls your endpoint when events arrive
- Unlike Service Bus, it's not a queue — events are pushed and not stored long-term

---

## Step 7 — Get Event Grid Endpoint and Key

```bash
# Endpoint URL
az eventgrid topic show \
  --name egt-learning-events \
  --resource-group rg-learn-phase4 \
  --query "endpoint" -o tsv

# Access key
az eventgrid topic key list \
  --name egt-learning-events \
  --resource-group rg-learn-phase4 \
  --query "key1" -o tsv
```

**Copy both to `context/project-context.md`.**

---

## Step 8 — Create Event Hubs Namespace

```bash
az eventhubs namespace create \
  --name ehns-learning-YOURNAME \
  --resource-group rg-learn-phase4 \
  --location eastus \
  --sku Basic
```

**Concepts:**
- Event Hubs is designed for **high-throughput streaming** (millions of events/second)
- Think: IoT sensors, application logs, telemetry pipelines
- Data is retained for 1–7 days (unlike a queue, consumers can replay)
- Uses **partitions** to parallelize processing

---

## Step 9 — Create Event Hub

```bash
az eventhubs eventhub create \
  --name telemetry-hub \
  --namespace-name ehns-learning-YOURNAME \
  --resource-group rg-learn-phase4 \
  --partition-count 2
```

`--partition-count 2` = two parallel lanes for processing. More partitions = more throughput.

---

## Step 10 — Get Event Hubs Connection String

```bash
az eventhubs namespace authorization-rule keys list \
  --resource-group rg-learn-phase4 \
  --namespace-name ehns-learning-YOURNAME \
  --name RootManageSharedAccessKey \
  --query "primaryConnectionString" -o tsv
```

**Copy to `context/project-context.md`.**

---

## Key Comparison: Service Bus vs Event Grid vs Event Hubs

| | Service Bus | Event Grid | Event Hubs |
|--|------------|-----------|-----------|
| **Pattern** | Message queue | Event routing | Data streaming |
| **Order** | Guaranteed (sessions) | Best-effort | Per-partition |
| **Retention** | Up to 14 days | 24 hours retry | 1–7 days |
| **Throughput** | Moderate | Moderate | Very high (millions/sec) |
| **Use when** | Reliable task processing | React to Azure events | IoT / telemetry / logs |

---

## ✅ Phase 4 Azure Setup Checklist

- [ ] Resource group `rg-learn-phase4` created
- [ ] Service Bus namespace (Basic) created
- [ ] Queue `orders-queue` created
- [ ] Service Bus connection string saved
- [ ] Tried Service Bus Explorer in portal
- [ ] Event Grid topic created
- [ ] Event Grid endpoint + key saved
- [ ] Event Hubs namespace (Basic) created
- [ ] Event Hub `telemetry-hub` created (2 partitions)
- [ ] Event Hubs connection string saved

---

## Now Start the Project

Tell Claude Code:
```
Start Phase 4 — create the messaging demos for Service Bus, Event Grid, and Event Hubs
```

---

## Cleanup

```bash
az group delete --name rg-learn-phase4 --yes --no-wait
```
