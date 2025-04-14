document.addEventListener('DOMContentLoaded', function () {
    const chatbox = document.getElementById('gemini-chatbox');
    const messagesContainer = document.getElementById('chatbox-messages');
    const input = document.getElementById('chatbox-input');
    const sendBtn = document.getElementById('chatbox-send-btn');
    const toggleBtn = document.getElementById('chatbox-toggle-btn');
    const closeBtn = document.getElementById('chatbox-close-btn');

    const apiEndpoint = '/api/chat'; // Route đã định nghĩa trong Controller

    // --- Hàm hiển thị tin nhắn ---
    function displayMessage(text, sender) { // sender là 'user' hoặc 'bot'
        const messageDiv = document.createElement('div');
        messageDiv.classList.add('message', `${sender}-message`);
        messageDiv.textContent = text;
        messagesContainer.appendChild(messageDiv);
        // Tự động cuộn xuống tin nhắn mới nhất
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
        return messageDiv; // Trả về element để có thể cập nhật (ví dụ: xóa thinking)
    }

    // --- Hàm hiển thị trạng thái đang chờ ---
    function showThinkingIndicator() {
        const thinkingDiv = displayMessage('', 'bot'); // Tạo div bot rỗng
        thinkingDiv.classList.add('thinking'); // Thêm class để CSS hiển thị dấu ...
        return thinkingDiv; // Trả về để có thể xóa đi sau
    }

    // --- Hàm gửi tin nhắn đến Backend ---
    async function sendMessage() {
        const messageText = input.value.trim();
        if (!messageText) return; // Không gửi nếu input rỗng

        // 1. Hiển thị tin nhắn người dùng
        displayMessage(messageText, 'user');
        input.value = ''; // Xóa input
        sendBtn.disabled = true; // Vô hiệu hóa nút gửi tạm thời
        input.disabled = true;   // Vô hiệu hóa input tạm thời

        // 2. Hiển thị trạng thái đang chờ
        const thinkingIndicator = showThinkingIndicator();

        try {
            // 3. Gọi API backend
            const response = await fetch(apiEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    // Không cần gửi API Key ở đây! Backend sẽ xử lý.
                    // Có thể thêm các header chống CSRF nếu cần
                    // 'RequestVerificationToken': getAntiForgeryToken() // Hàm lấy token nếu dùng anti-CSRF
                },
                body: JSON.stringify({ message: messageText }) // Gửi object chứa tin nhắn
            });

            // 4. Xóa trạng thái đang chờ
            messagesContainer.removeChild(thinkingIndicator);

            if (response.ok) {
                const data = await response.json();
                // 5. Hiển thị phản hồi của bot
                displayMessage(data.reply, 'bot');
            } else {
                // 6. Hiển thị lỗi nếu gọi API thất bại
                const errorData = await response.json().catch(() => ({ reply: 'Lỗi không xác định từ server.' })); // Bắt lỗi nếu response không phải JSON
                console.error("API Error:", response.status, errorData);
                displayMessage(`Lỗi: ${errorData.error || response.statusText || 'Không thể nhận phản hồi.'}`, 'bot');
            }

        } catch (error) {
            // 7. Xóa trạng thái đang chờ nếu có lỗi mạng
            if (thinkingIndicator && messagesContainer.contains(thinkingIndicator)) {
                messagesContainer.removeChild(thinkingIndicator);
            }
            console.error("Fetch Error:", error);
            displayMessage('Lỗi kết nối đến máy chủ.', 'bot');
        } finally {
            // 8. Kích hoạt lại input và nút gửi
            sendBtn.disabled = false;
            input.disabled = false;
            input.focus(); // Focus lại vào ô input
        }
    }

    // --- Xử lý sự kiện ---
    // Gửi khi nhấn nút
    sendBtn.addEventListener('click', sendMessage);

    // Gửi khi nhấn Enter trong ô input
    input.addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            sendMessage();
        }
    });

    // Mở/Đóng chatbox
    toggleBtn.addEventListener('click', () => {
        chatbox.classList.toggle('visible');
        if (chatbox.classList.contains('visible')) {
            input.focus(); // Focus vào input khi mở
            toggleBtn.textContent = 'Đóng Chat'; // Đổi chữ nút toggle
        } else {
            toggleBtn.textContent = 'Chat'; // Đổi lại chữ
        }
    });

    // Đóng chatbox bằng nút 'x'
    closeBtn.addEventListener('click', () => {
        chatbox.classList.remove('visible');
        toggleBtn.textContent = 'Chat'; // Đổi lại chữ nút toggle
    });

});