# Overview
Proyek ECommerce berbasis .NET Core 8, C#, SQL Server, dan Entity Framework Core.

# Database Design
Desain database dapat dilihat pada link berikut: [Database Design](https://dbdiagram.io/d/NET-E-Commerce-System-6736fb3ce9daa85aca8d36ae)  

# Feature
Berikut dijabarkan fitur-fitur yang tersedia yang berbasis UserRoles yang ada yaitu ADMIN dan CUSTOMER. Fitur yang dapat diakses oleh CUSTOMER juga dapat diakses oleh ADMIN, namun tidak sebaliknya.
## ADMIN
- Melihat data Customer.  
- Meng-upgrade Customer menjadi Admin.
- Menghapus data Customer.
- Melihat data Order dan OrderTransaction, mengubah status Order dan OrderTransaction milik Customer.  
- Menambahkan, mengubah, dan menghapus data ProductCategory dan data Product.
## CUSTOMER
- Membuat akun Customer, melakukan autentikasi, mengubah informasi akun Customer, dan melihat data akun Customer milik sendiri.
- Melihat data ProductCategory dan Product.
- Menambahkan Product ke dalam keranjang belanja (CustomerCart).
- Membeli Product yang telah dimasukkan ke dalam CustomerCart.
- Melihat riwayat Order milik Customer itu sendiri (CustomerOrderHistories).

