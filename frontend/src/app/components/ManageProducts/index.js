import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import moment from 'moment';
import SVG from 'app.dump/SVG';
/* components */
import Spinner from 'app.dump/Spinner';
/* ac */
import { loadManageProducts } from 'app.ac/manageProducts';

class ManageProducts extends Component {
  static propTypes = {
    isLoading: PropTypes.bool.isRequired,
    loadManageProducts: PropTypes.func.isRequired,
    tableHeaders: PropTypes.array.isRequired,
    templates: PropTypes.array.isRequired
  };

  constructor(props) {
    super(props);

    this.sortByColumn = this.sortByColumn.bind(this);

    this.state = {
      sortBy: '',
      sortOrderAsc: false,
      sortedTemplates: []
    };
  }

  sortByColumn(name) {
    this.setState({
      sortBy: name,
      sortOrderAsc: !this.state.sortOrderAsc,
      sortedTemplates: [...this.state.sortedTemplates].sort((template1, template2) => {
        if (template1[name] === null) {
          return Number.POSITIVE_INFINITY;
        }
        if (template2[name] === null) {
          return Number.NEGATIVE_INFINITY;
        }

        if (this.state.sortOrderAsc) {
          return template1[name] < template2[name];
        }
        return template1[name] > template2[name];
      })
    });
  }

  componentWillReceiveProps(nextProps) {
    // set initial sort values from tableHeaders
    nextProps.tableHeaders.find((column) => {
      if (column.sorting === 'asc' || column.sorting === 'desc') {
        this.setState({
          sortBy: column.name,
          sortOrderAsc: column.sorting === 'asc'
        });
        return true;
      }
      return false;
    });

    this.setState({
      sortedTemplates: [...nextProps.templates]
    });
  }

  componentDidMount() {
    // load Templates
    this.props.loadManageProducts();
  }

  render() {
    const { isLoading, tableHeaders } = this.props;
    const { sortBy, sortOrderAsc, sortedTemplates } = this.state;

    return (
      <div className="product-template__block">
        <div className="product-template__item">
          <h3>Manage products</h3>
        </div>
        <div className="product-template__item">

          {isLoading && <Spinner />}

          {!isLoading && <table className="table table--opposite table--inside-border table--hover product-list">
            <tbody>

            <tr>
              {tableHeaders.map(column => (
                <th key={column.name} onClick={() => this.sortByColumn(column.name)}>
                  {column.name === sortBy &&
                    (sortOrderAsc ? (
                      <span>
                        <SVG name="arrow" className="icon-modal" style={{ transform: 'rotate(180deg)' }} />
                      </span>
                    ) : (
                      <span>
                        <SVG name="arrow" className="icon-modal" />
                      </span>
                    ))
                  }
                  {column.title}
                </th>
              ))}
              <th></th>
            </tr>

            {sortedTemplates.map(template => (
              <tr key={template.templateId} className="product-list__row js-redirection" data-url="#">
                <td>
                  <a className="link weight--normal" href={template.editorUrl}>{template.productName}</a>
                </td>
                <td>
                  {template.createdDate && moment(template.createdDate).format('MMM D YYYY')}
                </td>
                <td>
                  {template.updatedDate && moment(template.updatedDate).format('MMM D YYYY')}
                </td>
                <td>
                  <div className="product-list__btn-group">
                    {/* TODO delete button */}
                    {/* <div className="confirmation js-confirmation" data-confirmation-active-element=".product-list__row" data-confirmation-active-class="product-list__row--hover" data-confirmation-button-text="Cancel" data-confirmation-position="top">
                      <button type="button" className="btn-main btn-main--secondary js-confirmation-clicker js-redirection-ignore">
                        Delete
                      </button>
                      <div className="js-confirmation-popper confirmation__popper">
                        <p>Sure you want to delete it?</p>
                        <button type="button" className="btn-main">Delete</button>
                      </div>
                    </div> */}
                    <a href={template.editorUrl} className="btn-action product-list__btn--primary">Open in design</a>
                  </div>
                </td>
              </tr>
            ))}
            </tbody>
          </table>}
        </div>

        {/* TODO Pagination component */}
      </div>
    );
  }
}

export default connect((state) => {
  const { isLoading, tableHeaders, templates } = state.manageProducts;
  return { isLoading, tableHeaders, templates };
}, {
  loadManageProducts
})(ManageProducts);