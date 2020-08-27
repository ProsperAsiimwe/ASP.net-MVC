$(function () {
    $('[data-toggle="popover"]').popover();
    $('[data-toggle="tooltip"]').tooltip();

    $('.nav.nav-tabs').tabCollapse({
        tabsClass: 'hidden-sm hidden-xs',
        accordionClass: 'visible-sm visible-xs'
    });

	$(".dial").knob({ 
		'readOnly' : true , 
		'angleOffset' : 180 , 
		'thickness' : .1, 
		'width' : '90%',
		'fgColor' : '#E0144E',
		'format' : function (value) {
			return value + '%';
		}
	});

	$(".sparkline").each(function(){
        var $data = $(this).data();

        $data.valueSpots = {'0:': $data.spotColor};
        $data.tooltipFormat =  "{{y:val}}";
        $data.fillColor = '#e8e8e8';

        $(this).sparkline( $data.data || "html", $data);
	});


	$('.filter-dropdown .dropdown-menu a').click(function() {
		var key = $(this).parent('li').parent('ul').attr('data-key');
		$('#' + key).val( $(this).attr('data-value') );
		 $(this).parent('li').parent('ul').parent('div').find('span').html( $(this).html() );
	})


	$(window).scroll(function(){
		if( $(this).scrollTop() > 200 ) {
			if( !$('.header').hasClass('scrolled') ) {
				$('.header').addClass('scrolled');
			}
		} else {
			if( $('.header').hasClass('scrolled') ) {
				$('.header').removeClass('scrolled')
			}
		}

		if( ($(this).scrollTop() + $(window).height()) > footerOffset && $(window).width() < 768 ) {
			if( !$(".sub-navigation").hasClass('unstuck') ) {
				$(".sub-navigation").addClass('unstuck')
			}
		} else {
			if( $(".sub-navigation").hasClass('unstuck') ) {
				$(".sub-navigation").removeClass('unstuck')
			}
		}
	});

    $(document).on('change', '.btn-file :file', function() {
	    var input = $(this),
	        numFiles = input.get(0).files ? input.get(0).files.length : 1,
	        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
	    console.log(label)
	    $(this).closest('.form-group').find('.upload-label').html(label);
	    //input.trigger('fileselect', [numFiles, label]);
	});


	//mobile subnav
	if($(window).width() < 768) {
		$(".sub-navigation").appendTo(".mobile-subnav");
	}
	$('.subnav-btn').click(function() {
		if($('.sub-navigation ul').is(':visible')) {
			$(this).html('+');
		} else {
			$(this).html('-');
		}
		$('.sub-navigation ul').slideToggle();
	})


	var footerOffset = $(".footer").offset().top;
	setTimeout(function() {
		footerOffset = $(".footer").offset().top + $(".sub-navigation").outerHeight();
	}, 500)

	$(window).resize(function(){
		footerOffset = $(".footer").offset().top + $(".sub-navigation").outerHeight();;
	})

});

$(document).on("click", ".js-tabcollapse-panel-heading", function () {
    if ($(window).width() < 768) {
        $('html, body').animate({
            scrollTop: $($(this)).offset().top
        }, 1200);
    }
});

function loadChart() {
    $(".sparkline").each(function () {
        var $data = $(this).data();

        $data.valueSpots = { '0:': $data.spotColor };
        $data.tooltipFormat = "{{y:val}}";
        $data.fillColor = '#e8e8e8';

        $(this).sparkline($data.data || "html", $data);
    });
}