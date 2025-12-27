# KPO_4

Проект реализует систему, которая выполняет упрощённые функции для интернет-магазина

# Запуск

```
docker compose up --build
```

# Swagger

Orders: http://localhost:8081/swagger/index.html

Payments: http://localhost:8082/swagger/index.html

Для всех запросов необходим
User-Id: (id пользователя)

# Payments

`POST /api/payments/accounts` - создать счёт

`POST /api/payments/accounts/topup` - пополнить баланс

`GET /api/payments/accounts/balance` - получить баланс

# Orders

`POST /api/orders/orders` - создать заказ

`GET /api/orders/orders` - список заказов

`GET /api/orders/orders/{id}` - статус заказа

# Пользовательский сценарий

Payments: `POST /accounts` -  `User-Id: petr`

Payments: `POST /accounts/topup` - `User-Id: petr`

Orders: `POST /orders` - `User-Id: petr`

Orders: `GET /orders/{id}` - `User-Id: petr`

Payments: `GET /accounts/balance`

# Статусы заказа

0 - заказ создан и ждёт результат оплаты

1 - оплата прошла успешно

2 - оплата не прошла 
