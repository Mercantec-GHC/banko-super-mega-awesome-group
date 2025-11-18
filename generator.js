function myFunction(elmnt) {
  console.log(elmnt.style.backgroundColor);
  if (elmnt.style.backgroundColor == "white") {
    elmnt.style.backgroundColor = "hsl(120, 100%, 75%)";
  } else {
    elmnt.style.backgroundColor = "white";
  }
}
var værdi;
var celle;
function nyfunktion(værdi, celle) {
  celle.innerHTML = værdi;
}

function gen_int(i) {
  var number = Math.floor(Math.random() * 10 + i * 10);
  while (number === 0) {
    var number = Math.floor(Math.random() * 10 + i * 10);
  }
  if (i === 8) {
    var number = Math.floor(Math.random() * 11 + 80);
  }
  if (i === 100) {
    var number = Math.ceil(Math.random() * 9);
  }
  return number;
}

function hasDuplicates(array) {
  return new Set(array).size !== array.length;
}

function generate_col(i) {
  var col = Array.from({ length: 3 }, () => gen_int(i));
  while (hasDuplicates(col) == true) {
    var col = Array.from({ length: 3 }, () => gen_int(i));
  }
  return col.sort();
}

function generate_row() {
  var row = Array.from({ length: 5 }, () => gen_int(100));
  while (hasDuplicates(row) == true) {
    var row = Array.from({ length: 5 }, () => gen_int(100));
  }
  return row.sort();
}

function contains_digits(rows) {
  var concat = rows[0].concat(rows[1], rows[2]);
  for (var i = 1; i < 9; i++) {
    if (concat.indexOf(i) === -1) {
      return false;
    }
  }

  if (concat.indexOf(9) === -1) {
    return false;
  }
  return true;
}
function generate_rows_check() {
  console.log(rows);
  var rows = new Array(generate_row(), generate_row(), generate_row());
  while (!contains_digits(rows)) {
    var rows = new Array(generate_row(), generate_row(), generate_row());
  }
  return rows;
}
function update_plates() {
  for (var x2 = 1; x2 <= 3; x2++) {
    for (var x3 = 1; x3 <= 9; x3++) {
      var celle = "p1" + String(x2) + String(x3);
      document.getElementById(celle).innerHTML = "";
    }
  }
  var dict = {};
  for (var n = 1; n <= 3; n++) {
    var cols = [];
    for (var i = 0; i < 9; i++) {
      var col = generate_col(i);
      cols.push(col);
    }

    var rows_choose = generate_rows_check();

    var chosen_row;

    for (var j = 0; j < 3; j++) {
      for (var i = 0; i < rows_choose[j].length; i++) {
        var k = rows_choose[j][i];
        var celle = "p" + String(n) + String(j + 1) + String(k);
        dict[celle] = cols[k - 1][j];
      }
    }
  }

  for (var key in dict) {
    var value = dict[key];
    nyfunktion(value, document.getElementById(key));
  }
}

function generateRandomName() {
    // Nulstil til ægte tilfældighed for navnevalg
    Math.seedrandom();
    
    // Liste af tilfældige navne
    const fornavne = ["William", "Karl", "Emil", "Oscar", "Malthe", "Noah", "Valdemar", "Aksel", "August", "Oliver", 
                      "Lucas", "Alfred", "Theo", "Elias", "Arthur", "Otto", "Elliot", "Felix", "Victor", "Magnus",
                      "Ella", "Freja", "Alma", "Frida", "Agnes", "Luna", "Ida", "Nora", "Olivia", "Sofia",
                      "Emma", "Clara", "Asta", "Alberte", "Karla", "Lily", "Ellie", "Anna", "Ellen", "Esther"];
    
    // Vælg tilfældigt fornavn
    const fornavn = fornavne[Math.floor(Math.random() * fornavne.length)];
    
    // Sæt navnet i tekstboksen
    document.getElementById('tekstboks').value = fornavn;
    
    // Sæt ny seed til pladegenerering
    Math.seedrandom(fornavn);
    update_plates();
}

document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('tekstboks').addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            Math.seedrandom(this.value);
            update_plates();
        }
    });
});
