# Payment Gateway API

## Overview

This payment gateway is an API-based application that enables merchants to process online payments from their customers. The system facilitates the interaction between shoppers, merchants, and acquiring banks to process card payments securely and efficiently.

### Key Entities

- **Shopper**: Individual buying the product online
- **Merchant**: Seller of the product (e.g., Apple, Amazon)
- **Payment Gateway**: Validates requests, stores card information, and handles payment processing with the acquiring bank
- **Acquiring Bank**: Processes the actual money transfer from shopper's card to merchant

## API Documentation

### Base URL
```
https://<api-domain>/api/Payments
```

### Endpoints

#### 1. Get Payment Details

Retrieve details about a specific payment transaction.

**GET** `/api/Payments/{id}`

**Parameters:**
- `id` (Guid): Unique ID of the payment transaction

**Response (200 OK):**
```json
{
    "id": "0bb07405-6d44-4b50-a14f-7ae0beff13ad",
    "status": "Authorized",
    "cardNumberLastFour": "8877",
    "expiryMonth": 4,
    "expiryYear": 2025,
    "currency": "GBP",
    "amount": 100
}
```

#### 2. Create Payment Request

Create a new payment request using card details.

**POST** `/api/Payments`

**Request Body:**
```json
{
    "cardNumber": "1234567812345678",
    "expiryMonth": 12,
    "expiryYear": 2025,
    "currency": "USD",
    "amount": 100,
    "cvv": "123"
}
```

**Required Fields:**
| Field | Type | Description |
|-------|------|-------------|
| cardNumber | string | Card number (14-19 numeric characters) |
| expiryMonth | int | Expiry month (1-12) |
| expiryYear | int | Expiry year (e.g., 2025) |
| currency | string | Currency code (EUR, USD, GBP) |
| amount | int | Transaction amount in minor currency unit (e.g., cents) |
| cvv | string | CVV (3-4 numeric characters) |

**Response (200 OK):**
```json
{
    "id": "a3f5b231-9c8d-4bdf-b9a2-7c57837c2c6e",
    "status": "Authorized",
    "cardNumberLastFour": "5678",
    "expiryMonth": 12,
    "expiryYear": 2025,
    "currency": "USD",
    "amount": 100
}
```

### Error Handling

The API uses standard HTTP status codes and returns detailed error information when needed:

- **400 Bad Request**: Validation errors in the input
- **404 Not Found**: Payment ID not found
- **500 Internal Server Error**: Unexpected server-side issues

**Error Response Example:**
```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "00-d7ddcb68b1f5fe0a90173574a0732249-d65aa060a310dd78-00",
    "errors": {
        "CardNumber": [
            "Card number is required."
        ]
    }
}
```

## Key Design Features

### Security
- Card numbers are stored securely using SecureString
- Only last four digits of card numbers are exposed in responses
- Secure transport protocols (HTTPS) required for all API communications
- Compliant with PCI DSS standards for card data handling

### Validation
- Comprehensive input validation for all payment fields
- Support for three major currencies (USD, EUR, GBP)
- Amount validation in minor currency units
- Card number and CVV format validation

### Technical Implementation
- In-memory storage for payment details
- Simulated acquiring bank integration for testing
- Detailed error logging (excluding sensitive data)
- Standard HTTP status codes and RESTful practices

## Amount Handling

Amounts should be provided in minor currency units:
- $0.01 = 1
- $10.50 = 1050

## Assumptions

- Merchants will store the payment ID for future reference
- Amounts will be submitted in minor currency units
- Only USD, EUR, and GBP are supported
- Secure transport protocols are enforced
- Card numbers will not be logged in plain text

## References

- [ISO 4217 Currency Codes](https://www.iso.org/iso-4217-currency-codes.html)
- [PCI DSS Security Standards](https://www.pcisecuritystandards.org/)
- [HTTP Status Codes Documentation](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)
