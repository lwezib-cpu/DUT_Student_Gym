// GymNet FAQ Chat Bot - rule-based (keyword matching), no external API,
// no cost, no signup. Runs entirely in the browser.
//
// TO EDIT THE ANSWERS: just change the FAQ array below. Each entry needs
// "keywords" (words/phrases that trigger it) and "answer" (what it replies
// with). No other code needs to change.
(function () {
    var FAQ = [
        {
            keywords: ["login", "log in", "sign in", "student email", "email format"],
            answer: "Log in with your student number (e.g. 21234567) or your full student email (studentnumber@dut4life.ac.za). Staff and trainers log in with their normal email instead."
        },
        {
            keywords: ["register", "sign up", "create account", "new account"],
            answer: "Go to the Register page and sign up with your DUT student email (studentnumber@dut4life.ac.za). You'll need your name, phone number and a password."
        },
        {
            keywords: ["forgot password", "reset password", "can't log in", "cant log in", "locked out"],
            answer: "There's no self-service password reset yet - please contact gym staff at the front desk and they can help you regain access."
        },
        {
            keywords: ["dashboard", "home page", "overview", "main page", "landing page"],
            answer: "Your Dashboard is the first page you see after logging in - it shows your membership status, days remaining, your consistency badge, and quick links to check in, fitness goals and more."
        },
        {
            keywords: ["my membership", "membership status", "days remaining", "days left", "when does my membership expire", "renew", "is my membership active"],
            answer: "Go to My Membership in the sidebar to see your current plan, status, and exactly how many days are left before it expires."
        },
        {
            keywords: ["payment history", "past payments", "my payments", "receipt", "receipts", "transaction"],
            answer: "Your Payment History page lists every payment you've made, with the date, amount and status of each one."
        },
        {
            keywords: ["membership", "plan", "plans", "price", "prices", "cost", "how much", "fee", "fees"],
            answer: "We have 3 plans: DUT Semester Plan (R250, 6 months), Monthly Plan (R80, available any time, 30 days), and Annual Plan (R650, 12 months, includes 2 free personal training sessions). Semester and Annual plans can only be purchased during the sign-up windows (Feb-Mar and Jul-Aug) - the Monthly Plan is available year-round."
        },
        {
            keywords: ["semester", "sign-up window", "sign up window", "when can i buy", "locked", "unlock"],
            answer: "Semester and Annual plans open for purchase in February-March (Semester 1) and July-August (Semester 2). Outside those windows they're locked, but the Monthly Plan is always available."
        },
        {
            keywords: ["check in", "checkin", "check-in", "qr code", "scan"],
            answer: "Go to Check In / Out in the sidebar and scan the QR code at the gym entrance, or ask a staff member to check you in manually. A timer starts automatically, and your Check-In History page shows a calendar of which days you've been in."
        },
        {
            keywords: ["check out", "checkout", "check-out", "leave the gym", "finish my session"],
            answer: "On the Check In / Out page, tap Check Out when you're done - it records how long you were at the gym. Staff can also check you out manually at the front desk."
        },
        {
            keywords: ["calendar", "check-in history", "checkin history", "attendance", "how many days"],
            answer: "Your Check-In History page shows a monthly calendar with every day you've checked in highlighted, plus your total time spent this month and all-time."
        },
        {
            keywords: ["equipment", "book equipment", "reserve", "reservation", "treadmill", "squat rack", "dumbbell", "kettlebell", "rowing machine", "bench press station", "maintenance"],
            answer: "Go to Equipment Booking, pick an item and how long you'll need it, then submit. Your request needs admin approval before it's confirmed, and staff will check you back in when you return the equipment. A reservation fee applies, plus an overuse fee if you keep it past your reserved time. If something's under maintenance it'll show as unavailable until staff mark it back in service."
        },
        {
            keywords: ["class", "classes", "join a class", "trainer class", "book a class", "waitlist"],
            answer: "Check the Classes page for upcoming sessions run by our trainers - each one shows the trainer's photo and specialty so you know who to look for. Tap Join Class to book your spot; if it's full you can join the waitlist and you'll be moved in automatically if a spot opens up."
        },
        {
            keywords: ["who is my trainer", "trainer photo", "meet the trainer", "what does my trainer look like", "trainers"],
            answer: "Check the 'Our Trainers' section on the homepage, or look at the Classes page - every class shows the trainer's photo and specialty (Yoga, Fitness, Cardio, or Basic Strength Training) so you can recognise them at the gym."
        },
        {
            keywords: ["fitness goal", "weight", "bmi", "measurement", "log my weight", "progress"],
            answer: "Go to Fitness Goals to log your weight and height (we calculate your BMI automatically) and set a target. Come back after about a month to see your progress, and check the Meal Recommendations section for food ideas that match your goal."
        },
        {
            keywords: ["meal", "food", "diet", "nutrition", "calories", "snack", "sweets", "build a meal"],
            answer: "On the Fitness Goals page you'll find meal ideas matched to your goal, plus a 'Build a Meal' tool where you pick ingredients and see an estimated calorie count. There's also general guidance on snacks and sweets in moderation."
        },
        {
            keywords: ["exercise", "recommend", "workout", "chest", "back", "legs", "arms", "shoulders", "core", "cardio", "add to workout", "start session"],
            answer: "Visit the Exercises page, pick a body part, and add exercises to your workout using the 'Add to Workout' button. Once you've picked a few, tap Start Session for a guided walkthrough - it times bodyweight moves, asks cardio machines how long you went for, and asks weighted exercises for sets/reps/weight, then saves it all to your workout history."
        },
        {
            keywords: ["badge", "reward", "streak", "consistency", "points"],
            answer: "Your dashboard shows a badge (Newcomer, Bronze, Silver, Gold, Platinum) based on your total check-ins. Keep coming back to level up!"
        },
        {
            keywords: ["feedback", "trainer feedback", "trainer notes"],
            answer: "If you've joined a trainer's class, they may leave you feedback - check the Trainer Feedback page in your sidebar."
        },
        {
            keywords: ["hours", "opening", "closing", "open", "close", "when are you open"],
            answer: "[Placeholder - ask gym staff to confirm exact opening hours, then update this answer in Scripts/chatbot.js]"
        },
        {
            keywords: ["location", "address", "where are you", "find you"],
            answer: "[Placeholder - update this answer in Scripts/chatbot.js with the real gym address/location on campus]"
        },
        {
            keywords: ["contact", "phone number", "email support", "help", "staff"],
            answer: "You can reach us at gym@dut.ac.za or visit the front desk during opening hours."
        },
        {
            keywords: ["hi", "hello", "hey", "howzit"],
            answer: "Hi! I can help with logins, memberships, check-in/out, equipment booking, classes, fitness goals and more. What do you need?"
        },
    ];

    var FALLBACK = "I'm not sure about that one - please ask a staff member at the front desk, or try rephrasing your question.";

    function findAnswer(text) {
        var lower = text.toLowerCase();
        var best = null;
        var bestScore = 0;
        for (var i = 0; i < FAQ.length; i++) {
            var score = 0;
            for (var k = 0; k < FAQ[i].keywords.length; k++) {
                if (lower.indexOf(FAQ[i].keywords[k]) !== -1) score++;
            }
            if (score > bestScore) {
                bestScore = score;
                best = FAQ[i];
            }
        }
        return best ? best.answer : FALLBACK;
    }

    function addMessage(text, fromBot) {
        var log = document.getElementById('gnChatLog');
        if (!log) return;
        var row = document.createElement('div');
        row.className = 'gn-chat-row ' + (fromBot ? 'gn-chat-bot' : 'gn-chat-user');
        var bubble = document.createElement('div');
        bubble.className = 'gn-chat-bubble';
        bubble.textContent = text;
        row.appendChild(bubble);
        log.appendChild(row);
        log.scrollTop = log.scrollHeight;
    }

    function sendMessage() {
        var input = document.getElementById('gnChatInput');
        if (!input || !input.value.trim()) return;
        var text = input.value.trim();
        addMessage(text, false);
        input.value = '';
        setTimeout(function () {
            var answer = findAnswer(text);
            addMessage(answer, true);
            speak(answer);
        }, 350);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var toggle = document.getElementById('gnChatToggle');
        var panel = document.getElementById('gnChatPanel');
        if (!toggle || !panel) return;

        var opened = false;
        toggle.addEventListener('click', function () {
            opened = !opened;
            panel.style.display = opened ? 'flex' : 'none';
            if (opened && document.getElementById('gnChatLog').children.length === 0) {
                addMessage("Hi! I'm the GymNet assistant. Ask me about memberships, check-in, equipment booking, classes or your fitness goals.", true);
            }
        });

        var closeBtn = document.getElementById('gnChatClose');
        if (closeBtn) {
            closeBtn.addEventListener('click', function () {
                opened = false;
                panel.style.display = 'none';
                stopListening();
            });
        }

        var sendBtn = document.getElementById('gnChatSend');
        if (sendBtn) sendBtn.addEventListener('click', sendMessage);

        var input = document.getElementById('gnChatInput');
        if (input) {
            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') sendMessage();
            });
        }

        setupVoice();
        setupVoiceToggle();
    });

    // ===== Voice on/off toggle - lets the person choose whether answers are
    // read aloud or shown as text only. Remembered per-browser. =====
    var voiceEnabled = true;

    function setupVoiceToggle() {
        var toggleBtn = document.getElementById('gnChatVoiceToggle');
        if (!toggleBtn) return;

        var stored = localStorage.getItem('gymnet-chat-voice');
        voiceEnabled = stored !== 'off';
        applyVoiceToggleIcon(toggleBtn);

        toggleBtn.addEventListener('click', function () {
            voiceEnabled = !voiceEnabled;
            localStorage.setItem('gymnet-chat-voice', voiceEnabled ? 'on' : 'off');
            applyVoiceToggleIcon(toggleBtn);
            if (!voiceEnabled && 'speechSynthesis' in window) {
                window.speechSynthesis.cancel();
            }
        });
    }

    function applyVoiceToggleIcon(btn) {
        btn.classList.toggle('gn-voice-off', !voiceEnabled);
        btn.title = voiceEnabled ? 'Voice answers on - tap to mute' : 'Voice answers off - tap to unmute';
        btn.innerHTML = voiceEnabled
            ? '<svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M4 9v6h4l5 5V4L8 9H4z"/><path d="M16.5 12a4.5 4.5 0 0 0-2.5-4.03v8.06A4.5 4.5 0 0 0 16.5 12z"/><path d="M14 4.55v2.06a7 7 0 0 1 0 10.78v2.06A9 9 0 0 0 14 4.55z"/></svg>'
            : '<svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M4 9v6h4l5 5V4L8 9H4z"/><path d="M15.5 8.5l5 5m0-5l-5 5" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>';
    }

    // ===== Voice input (speech-to-text) and voice output (text-to-speech) =====
    // Both use the browser's built-in Web Speech API - no external service,
    // no API key, no cost. Not every browser supports SpeechRecognition
    // (Chrome/Edge do; Firefox mostly doesn't), so the mic button is hidden
    // automatically when it's not available rather than showing something broken.
    var recognition = null;
    var listening = false;

    function setupVoice() {
        var micBtn = document.getElementById('gnChatMic');
        if (!micBtn) return;

        var SpeechRecognitionApi = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognitionApi) {
            micBtn.style.display = 'none'; // not supported in this browser - hide rather than show a dead button
            return;
        }

        recognition = new SpeechRecognitionApi();
        recognition.lang = 'en-ZA';
        recognition.interimResults = false;
        recognition.maxAlternatives = 1;

        recognition.onresult = function (event) {
            var transcript = event.results[0][0].transcript;
            var input = document.getElementById('gnChatInput');
            if (input) input.value = transcript;
            sendMessage();
        };

        recognition.onend = function () {
            listening = false;
            micBtn.classList.remove('gn-mic-active');
        };

        recognition.onerror = function () {
            listening = false;
            micBtn.classList.remove('gn-mic-active');
        };

        micBtn.addEventListener('click', function () {
            if (listening) {
                stopListening();
            } else {
                listening = true;
                micBtn.classList.add('gn-mic-active');
                recognition.start();
            }
        });
    }

    function stopListening() {
        if (recognition && listening) {
            recognition.stop();
            listening = false;
        }
    }

    function speak(text) {
        if (!voiceEnabled) return; // person has muted voice answers
        if (!('speechSynthesis' in window)) return; // not supported - answer still shows as text, just not read aloud
        var utterance = new SpeechSynthesisUtterance(text);
        utterance.rate = 1;
        utterance.pitch = 1;
        window.speechSynthesis.cancel(); // stop any answer still being read before starting the next one
        window.speechSynthesis.speak(utterance);
    }
})();
