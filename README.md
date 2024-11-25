# Overview
Proyek ECommerce berbasis .NET Core 8, C#, SQL Server, dan Entity Framework Core.

# Database Design
Desain database dapat dilihat pada link berikut: [Database Design](https://dbdiagram.io/d/NET-E-Commerce-System-6736fb3ce9daa85aca8d36ae)  
Secara ringkas sebagai berikut:
### Desain Tabel
#### **1. Data Pelanggan**

-   **Tabel `Customers`**: Berisi informasi dasar pelanggan seperti nama, email, kata sandi, dan peran (CUSTOMER atau ADMIN). Setiap pelanggan memiliki catatan waktu kapan mereka terdaftar.
-   **Tabel `CustomerAddresses`**: Menyimpan alamat pelanggan, termasuk negara, provinsi, dan alamat lengkap. Alamat ini terkait dengan pelanggan tertentu.

#### **2. Data Produk**

-   **Tabel `Products`**: Menyimpan informasi produk, seperti harga, stok, deskripsi, kategori, dan diskon yang berlaku. Ada catatan waktu kapan produk ditambahkan ke sistem.
-   **Tabel `ProductCategories`**: Mengelompokkan produk berdasarkan kategori, dengan deskripsi kategori dan diskon tambahan jika ada.

#### **3. Data Keranjang Belanja**

-   **Tabel `CustomerCart`**: Menyimpan data produk yang ada di keranjang belanja pelanggan, termasuk jumlah barang dan total harganya.

#### **4. Data Pemesanan**

-   **Tabel `Orders`**: Menyimpan informasi pesanan, termasuk jumlah total, status pesanan (menunggu pembayaran, dalam proses, dikirim, atau selesai), tenggat waktu, dan pelanggan yang memesan.
-   **Tabel `OrderItems`**: Merinci setiap produk dalam pesanan tertentu, termasuk jumlah dan harga masing-masing barang.
-   **Tabel `CustomerOrderHistories`**: Menghubungkan pelanggan dengan pesanan-pesanan mereka.

#### **5. Data Transaksi**

-   **Tabel `OrderTransactions`**: Menyimpan data transaksi pembayaran untuk pesanan, termasuk metode pembayaran (bank, kartu kredit, atau e-wallet) dan status pembayaran (menunggu, gagal, atau berhasil).

#### **6. Enums untuk Standarisasi Data**

-   **`UserRoles`**: Menentukan peran pengguna, yaitu pelanggan (CUSTOMER) atau admin (ADMIN).
-   **`OrderStatus`**: Mengelola status pesanan, seperti menunggu pembayaran, dalam proses, dikirim, atau selesai.
-   **`PaymentStatuses`**: Mengelola status pembayaran, seperti menunggu, gagal, atau berhasil.
-   **`PaymentMethods`**: Menentukan metode pembayaran yang digunakan.

### Relasi Antar Tabel

#### **1. Relasi Pelanggan dan Alamat**

-   Setiap pelanggan dapat memiliki **banyak alamat**.
-   Tabel `CustomerAddresses` terhubung ke tabel `Customers` melalui kolom `CustomerId`. Ini memastikan setiap alamat terkait dengan pelanggan tertentu.

#### **2. Relasi Produk dan Kategori**

-   Setiap produk masuk ke dalam **satu kategori**.
-   Tabel `Products` terhubung ke tabel `ProductCategories` melalui kolom `CategoryId`. Hal ini memungkinkan pengelompokan produk berdasarkan jenis atau kategori tertentu.

#### **3. Relasi Keranjang Belanja**

-   Setiap pelanggan dapat memiliki **banyak item di keranjang belanja**.
-   Tabel `CustomerCart` terhubung ke tabel `Customers` melalui kolom `CustomerId` dan ke tabel `Products` melalui kolom `ProductId`. Ini mencatat produk mana yang dimasukkan ke keranjang oleh pelanggan.

#### **4. Relasi Pesanan**

-   Setiap pesanan dibuat oleh **satu pelanggan**.
-   Tabel `Orders` terhubung ke tabel `Customers` melalui kolom `CustomerId`.
-   Satu pesanan dapat memiliki **banyak produk**, yang tercatat dalam tabel `OrderItems`.
    -   `OrderItems` terhubung ke tabel `Orders` melalui kolom `OrderId` dan ke tabel `Products` melalui kolom `ProductId`.

#### **5. Relasi Riwayat Pesanan**

-   Setiap pelanggan memiliki **riwayat pesanan**.
-   Tabel `CustomerOrderHistories` menghubungkan tabel `Customers` dan `Orders`, mencatat pesanan yang dibuat oleh pelanggan tertentu.

#### **6. Relasi Transaksi**

-   Setiap pesanan memiliki **satu transaksi pembayaran**.
-   Tabel `OrderTransactions` terhubung ke tabel `Orders` melalui kolom `OrderId`. Transaksi mencatat metode pembayaran dan status pembayaran untuk pesanan tertentu.

# Feature
Penjabaran fitur pada API dibagi menjadi basis Endpoint API dan basis non-Endpoint API.
## Feature Based API Endpoint
Fitur pada seksi ini dapat langsung diakses pada Postman Collection yang sudah disediakan pada Repository.
	-	Import file Postman_Collection.json
	-	Klik kanan pada Postman Collection tersebut dan pilih `View Documentation`
<a href="https://ibb.co.com/T4xzxyY"><img src="https://i.ibb.co.com/3p9t9K4/image.png" alt="image" border="0"></a>  
	- Kemudian akan tampil dokumentasi terkait seluruh API Endpoints
<a href="https://ibb.co.com/wyQX8Vb"><img src="https://i.ibb.co.com/CB9Z4gX/image.png" alt="image" border="0"></a>
## Feature Based Non-Endpoint
Fitur pada seksi ini berupa 2 Background Service yaitu:
- Async Queue Send Email (ada BUG yaitu prosesnya sukses tapi email yang dikirimkan pesan ga nerima apa-apa)
	Implementasi pengiriman email seperti saat Customer menyelesaikan proses pembayaran lalu API mengirim email notifikasi ke Admin dan saat Admin mengubah status Order milik Customer lalu API mengirimkan email notifkasi ke Customer. Fitur ini Async Queue yang artinya diproses secara asinkron (tidak pada Thread utama) dan berbentuk Queue (antrian) sehingga akan dijalankan secara FIFO (First In First Out).
- Product Restock
	Implementasi pengembalian stock Product berdasarkan setiap Quantity OrderItems yang telah disimpan oleh Customer. Operasi yang dilakukan berupa setiap 1 jam API akan menarik data Order yang telah melewati masa Deadline-nya dan status Order masih dalam WAITING_PAYMENT (kita sebut Expired Order). Kemudian akan diambil data OrderItems yaitu Product-Product yang dipilih Customer dan berkaitan dengan setiap Expired Order sebelumnya, dan dilakukan penambahan Product Stock.

# How To Run
- Clone repository ini kemudian buat file`appsettings.[Environment].json`dan lengkapi datanya berdasarkan `appsettings.Example.json` yang telah disediakan. 
- Pada terminal jalankan `dotnet restore` untuk mengunduh packages yang dibutuhkan. Lalu jalankan `dotnet run` untuk menjalankan API.
- Untuk mencoba API import Postman Collection yang telah disediakan pada aplikasi Postman/Insomnia/etc. Lalu baca dokumentasi yang disediakan.

