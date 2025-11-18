let availableNumbers = [];
let calledNumbers = [];
let currentNumber = null;

function initializeGame() {
    // Prøv at hente gemt spiltilstand
    const savedState = loadGameState();
    
    if (savedState) {
        availableNumbers = savedState.availableNumbers;
        calledNumbers = savedState.calledNumbers;
        currentNumber = savedState.currentNumber;
    } else {
        // Hvis ingen gemt tilstand, start nyt spil
        availableNumbers = Array.from({length: 90}, (_, i) => i + 1);
        calledNumbers = [];
        currentNumber = null;
    }
    
    // Opret grid med alle numre
    const grid = document.getElementById('numberGrid');
    grid.innerHTML = '';
    
    for (let i = 1; i <= 90; i++) {
        const cell = document.createElement('div');
        cell.className = 'number-cell';
        cell.id = `number-${i}`;
        cell.textContent = i;
        if (calledNumbers.includes(i)) {
            cell.classList.add('called');
        }
        if (currentNumber === i) {
            cell.classList.add('current');
        }
        grid.appendChild(cell);
    }
    
    updateDisplay();
}

function drawNumber() {
    if (availableNumbers.length === 0) {
        alert('Alle numre er trukket!');
        return;
    }
    
    // Fjern 'current' class fra forrige nummer hvis det findes
    if (currentNumber) {
        document.getElementById(`number-${currentNumber}`).classList.remove('current');
        document.getElementById(`number-${currentNumber}`).classList.add('called');
    }
    
    // Træk tilfældigt nummer
    const randomIndex = Math.floor(Math.random() * availableNumbers.length);
    currentNumber = availableNumbers.splice(randomIndex, 1)[0];
    calledNumbers.push(currentNumber);
    
    // Gem spiltilstand
    saveGameState();
    
    // Opdater visning
    updateDisplay();
}

function updateDisplay() {
    // Opdater det aktuelle nummer
    const currentNumberDisplay = document.getElementById('currentNumber');
    currentNumberDisplay.textContent = currentNumber ? `Nummer: ${currentNumber}` : 'Tryk på "Træk nyt nummer" for at starte';
    
    // Opdater grid
    if (currentNumber) {
        const currentCell = document.getElementById(`number-${currentNumber}`);
        currentCell.classList.add('current');
    }
    
    // Opdater historik
    const history = document.getElementById('numberHistory');
    history.innerHTML = calledNumbers.map((num, index) => 
        `${index + 1}: ${num}`
    ).join('<br>');
}

function resetGame() {
    if (confirm('Er du sikker på at du vil starte et nyt spil?')) {
        // Ryd gemt spiltilstand
        localStorage.removeItem('bankoGameState');
        initializeGame();
    }
}

function showVerifyDialog() {
    document.getElementById('verifyDialog').style.display = 'flex';
    document.getElementById('verifyResult').style.display = 'none';
    document.getElementById('playerId').value = '';
}

function closeVerifyDialog() {
    document.getElementById('verifyDialog').style.display = 'none';
}

function verifyBanko() {
    const playerId = document.getElementById('playerId').value;
    const winType = document.getElementById('winType').value;
    
    if (!playerId) {
        showVerifyResult('Indtast venligst et spiller-ID', false);
        return;
    }

    console.log('Verificerer for spiller:', playerId);
    
    // Gem den nuværende tilfældige tilstand
    const currentRandom = Math.random;
    
    // Sæt den deterministiske generator op med spillerens ID
    Math.seedrandom(playerId);
    
    // Generer spillerens plade
    const playerPlate = generatePlate();
    console.log('Genereret plade:', playerPlate);
    
    // Gendan den oprindelige tilfældige generator
    Math.random = currentRandom;

    // Vis pladen
    displayVerifyPlate(playerPlate);
    console.log('Kaldte numre:', calledNumbers);

    // Verificér pladen mod de kaldte numre
    const result = checkWin(playerPlate, winType);
    showVerifyResult(result.message, result.success);
}

function displayVerifyPlate(plate) {
    const plateDiv = document.getElementById('verifyPlate');
    plateDiv.style.display = 'block';
    
    let html = '<table>';
    for (let row = 0; row < 3; row++) {
        html += '<tr>';
        for (let col = 0; col < 9; col++) {
            const number = plate[row][col];
            let className = 'empty';
            
            if (number) {
                className = calledNumbers.includes(number) ? 'called' : 'not-called';
            }
            
            html += `<td>
                ${number ? `<div class="verify-number ${className}">${number}</div>` : ''}
            </td>`;
        }
        html += '</tr>';
    }
    html += '</table>';
    
    plateDiv.innerHTML = html;
}

function checkWin(plate, winType) {
    if (calledNumbers.length === 0) {
        return {
            success: false,
            message: 'Der er ikke trukket nogen numre endnu!'
        };
    }

    let rowsCompleted = 0;
    
    // Tjek hver række
    for (let row = 0; row < 3; row++) {
        let rowComplete = true;
        for (let col = 0; col < 9; col++) {
            if (plate[row][col] && !calledNumbers.includes(plate[row][col])) {
                rowComplete = false;
                break;
            }
        }
        if (rowComplete) rowsCompleted++;
    }

    // Returner resultat baseret på winType
    switch(winType) {
        case 'row':
            if (rowsCompleted >= 1) {
                return {
                    success: true,
                    message: 'Tillykke! Du har banko på 1 række!'
                };
            }
            break;
        case 'rows2':
            if (rowsCompleted >= 2) {
                return {
                    success: true,
                    message: 'Tillykke! Du har banko på 2 rækker!'
                };
            }
            break;
        case 'full':
            if (rowsCompleted === 3) {
                return {
                    success: true,
                    message: 'Tillykke! Du har banko på fuld plade!'
                };
            }
            break;
    }

    return {
        success: false,
        message: 'Der er desværre ikke banko endnu. Prøv igen senere!'
    };
}

function generatePlate() {
    var dict = {};
    var cols = [];
    
    // Generer kolonner
    for (var i = 0; i < 9; i++) {
        var col = generate_col(i);
        cols.push(col);
    }

    var rows_choose = generate_rows_check();
    
    // Opret tom plade
    let plate = Array(3).fill().map(() => Array(9).fill(0));
    
    // Udfyld pladen
    for (var j = 0; j < 3; j++) {
        for (var i = 0; i < rows_choose[j].length; i++) {
            var k = rows_choose[j][i];
            plate[j][k-1] = cols[k-1][j];
        }
    }

    return plate;
}

function showVerifyResult(message, success) {
    const resultDiv = document.getElementById('verifyResult');
    resultDiv.textContent = message;
    resultDiv.className = 'verify-result ' + (success ? 'success' : 'error');
    resultDiv.style.display = 'block';
}

function updateHistory(number) {
    const historyDiv = document.getElementById('numberHistory');
    const numberElement = document.createElement('div');
    numberElement.className = 'history-number';
    numberElement.textContent = number;
    historyDiv.insertBefore(numberElement, historyDiv.firstChild);
}

// Tilføj disse nye funktioner til at håndtere gem/hent af spiltilstand
function saveGameState() {
    const gameState = {
        availableNumbers,
        calledNumbers,
        currentNumber
    };
    localStorage.setItem('bankoGameState', JSON.stringify(gameState));
}

function loadGameState() {
    const savedState = localStorage.getItem('bankoGameState');
    return savedState ? JSON.parse(savedState) : null;
}

// Initialiser spillet når siden loader
document.addEventListener('DOMContentLoaded', initializeGame); 